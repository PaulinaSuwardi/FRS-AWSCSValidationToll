using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using log4net;
using System.IO;
using System.Collections.Concurrent;
using System.Configuration;
using Amib.Threading;
using System.Threading;

namespace FRS_AWSCSSync
{
    public class ProcessMgr
    {
        private FileMetaDB _fileMetaDB;
        private DynamoClientWrapper _dynamoClient;
        private S3ClientWrapper _s3Client;
        private static ILog _logger;
        private BlockingCollection<String> _workQueue;
        private SmartThreadPool _dbReaderThreadPool;
        private SmartThreadPool _awsQuerierThreadPool;
        private ManualResetEvent _manualEvent;

        public ProcessMgr()
        {
            _logger = LogManager.GetLogger(String.Empty);
            _fileMetaDB = new FileMetaDB(AppConfigClass.DBConnectionStr, AppConfigClass.DBReconnectCount);
            _s3Client = new S3ClientWrapper(RegionEndpoint.USEast1, "cs-trend-prod");
            _dynamoClient = new DynamoClientWrapper(RegionEndpoint.USEast1, "cs-file-metadata-prod", "IX_SHA1");
            _workQueue = new BlockingCollection<String>();
            _dbReaderThreadPool = new SmartThreadPool(60000, AppConfigClass.DBReaderThreadCount, 0);
            _awsQuerierThreadPool = new SmartThreadPool(60000, AppConfigClass.AwsCheckerThreadCount, 0);
            _manualEvent = new ManualResetEvent(false);
        }

        public void Terminate()
        {
            _manualEvent.Set();
        }

        private bool IsTerminate()
        {
            if (_manualEvent.WaitOne(0))
            {
                return true;
            }
            return false;
        }

        public void Start()
        {
            _logger.Info("Start Process");

            if (AppConfigClass.DeleteLastMark)
            {
                _fileMetaDB.DeleteLastMark();
            }

            MultiThreadGetSHA1FromDBToQueue();
            MultiThreatCheckFileInQueue();
        }

        public void MultiThreadGetSHA1FromDBToQueue()
        {
            for (int i = 0; i < AppConfigClass.DBReaderThreadCount; i++)
            {
                _dbReaderThreadPool.QueueWorkItem(GetSHA1FromDBToQueue, String.Format("{0}{1}",AppConfigClass.DBReaderName, i));
            }
        }

        public void GetSHA1FromDBToQueue(String readerName)
        {
            List<String> sha1Lst = _fileMetaDB.GetSHA1List(AppConfigClass.NumberOfDataPerDBQuery, readerName);

            while (sha1Lst.Count > 0)
            {
                IsTerminate();
               
                _logger.InfoFormat("Get {0} sha1 from DB", sha1Lst.Count);
                foreach (var sha1 in sha1Lst)
                {
                    _workQueue.Add(sha1);
                }

                _manualEvent.WaitOne(1000 * AppConfigClass.DBReaderThreadWaitInSec);

                sha1Lst = _fileMetaDB.GetSHA1List(AppConfigClass.NumberOfDataPerDBQuery, readerName);
            }
        }

        public void MultiThreatCheckFileInQueue()
        {
            for (int i = 0; i < AppConfigClass.AwsCheckerThreadCount; i++)
            {
                _awsQuerierThreadPool.QueueWorkItem(ConsumeWorkQueue); 
            }
        }

        public void ConsumeWorkQueue()
        {
            foreach (string sha1 in _workQueue.GetConsumingEnumerable())
            {
                IsTerminate();
                CheckFileInAWS(sha1);
            }
        }

        public void CheckFileInAWS(string sha1)
        {
            S3Result s3Result = CheckExistInS3(String.Format("frs/{0}/{1}/{2}/{3}/{4}", sha1.Substring(0, 2), sha1.Substring(2, 3), sha1.Substring(5, 3), sha1.Substring(8, 5), sha1));
            
            int dynamoResult = CheckExistInDynamo(sha1);    
            
            _fileMetaDB.SetValidateAWSResult(sha1, (int)s3Result, dynamoResult);
        }

        public S3Result CheckExistInS3(string fileKey)
        {
            S3Result result = _s3Client.CheckS3File(fileKey);
            
            _logger.InfoFormat("SHA1:{0} {1} in S3", Path.GetFileName(fileKey) , result.ToString());
            
            return result;
        }

        public int CheckExistInDynamo(string sha1)
        {
            int recordCount;

            if (_dynamoClient.GetDynamoRecordCount(sha1, out recordCount))
            {
                _logger.InfoFormat("SHA1:{0} recordCount:{1} in dynamo", sha1, recordCount);

                return recordCount;
            }
            else
            {
                _logger.InfoFormat("SHA1:{0} check dynamo fail", sha1);
                return -1;
            }
        }
    }
}
