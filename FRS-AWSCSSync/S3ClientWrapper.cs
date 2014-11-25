using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon;
using log4net;
using Amazon.S3.Model;
using Amazon.Runtime;

namespace FRS_AWSCSSync
{
    public enum S3Result
    { 
        NOT_EXIST = 0,
        EXIST = 1,
        QUERY_FAIL = -1
    }
    public class S3ClientWrapper
    {
        private RegionEndpoint _awsRegion;
        private ILog _logger;
        private string _bucketName;

        public S3ClientWrapper(RegionEndpoint awsRegion, string bucketName)
        {
            _logger = LogManager.GetLogger(String.Empty);
            _awsRegion = awsRegion;
            _bucketName = bucketName;
        }

        public S3Result CheckS3File(string fileKey)
        {
            using (AmazonS3Client client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
            {
                _logger.InfoFormat("CheckS3File fileKey: {0}", fileKey);

                try
                {
                    GetObjectMetadataResponse response = client.GetObjectMetadata(_bucketName, fileKey);

                    //TO-DO : print some metadata

                    return S3Result.EXIST;
                }
                catch (AmazonServiceException e)
                {
                    if (e.ErrorCode.Equals("NoSuchKey") || e.ErrorCode.Equals("NotFound"))
                    {
                        return S3Result.NOT_EXIST;
                    }

                    _logger.ErrorFormat("[CheckS3File] {0} Exception code: {1}; type:{2}; msg:{3}", fileKey, e.ErrorCode, e.ErrorType, e.Message);
                }
            }
            return S3Result.QUERY_FAIL;
        }
    }
}
