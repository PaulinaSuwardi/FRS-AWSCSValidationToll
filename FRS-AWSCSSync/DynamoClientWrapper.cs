using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon;
using log4net;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

namespace FRS_AWSCSSync
{
    public class DynamoClientWrapper
    {
        private RegionEndpoint _awsRegion;
        private ILog _logger;
        private string _tableName;
        private string _indexName;

        public DynamoClientWrapper(RegionEndpoint awsRegion, string tableName, string indexName)
        {
            _logger = LogManager.GetLogger(String.Empty);
            _awsRegion = awsRegion;
            _tableName = tableName;
            _indexName = indexName;
        }

        public bool GetDynamoRecordCount(string sha1, out int recordCount)
        {
            recordCount = 0;
            using (AmazonDynamoDBClient client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1))
            {
                _logger.InfoFormat("checkDynamoRecord sha1: {0}", sha1);

                try
                {
                    QueryRequest queryRequest = new QueryRequest
                    {
                        TableName = _tableName,
                        IndexName = _indexName,
                        ScanIndexForward = true
                    };

                    Dictionary<String, Condition> keyConditions = new Dictionary<String, Condition>();

                    keyConditions.Add(
                        "SHA1",
                        new Condition
                        {
                            ComparisonOperator = "EQ",
                            AttributeValueList = { new AttributeValue { S = sha1 } }
                        }
                    );

                    queryRequest.KeyConditions = keyConditions;

                    var result = client.Query(queryRequest);

                    if (result == null)
                        return false;

                    recordCount = result.Count;

                    return true;
                }
                catch (AmazonServiceException e)
                {
                    _logger.ErrorFormat("[CheckDynamoRecord] {0} Exception code:{1}; type:{2}; msg:{3}", sha1, e.ErrorCode, e.ErrorType, e.Message);
                }
            }
            return false;
        }
    }
}
