using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using log4net;

namespace FRS_AWSCSSync
{
    public class FileMetaDB
    {
        private int _reconnectCount;
        private string _dbConnStr;
        private static ILog _logger;

        public FileMetaDB(String connectionStr, int reconnectCount)
        {
            _logger = LogManager.GetLogger(String.Empty);
            _dbConnStr = connectionStr;
            _reconnectCount = reconnectCount;
        }

        public List<String> GetSHA1List(int dataCount, string readerName)
        {
            List<String> shaList = new List<string>();

            for (int i = 0; i < _reconnectCount; i++)
            {
                using (SqlConnection conn = new SqlConnection(_dbConnStr))
                {
                    try
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand("spValidateGetSHA1List", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@numberOfData", dataCount));
                            cmd.Parameters.Add(new SqlParameter("@readerName", readerName));

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    shaList.Add(reader["SHA1"].ToString());
                                }

                                return shaList;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorFormat("[FileMetaDB] GetFileMetaList exception:{0}", e.Message);
                    }
                }
            }

            _logger.ErrorFormat("[FileMetaDB] GetFileMetaList ERROR: DB has been reconnect {0} but fail", _reconnectCount);
            return shaList;
        }

        public Boolean SetValidateAWSResult(String sha1, int s3Result, int dynamoResult)
        {
            for (int i = 0; i < _reconnectCount; i++)
            {
                using (SqlConnection conn = new SqlConnection(_dbConnStr))
                {
                    try
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand("spValidateAWSResult", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@sha1", sha1));
                            cmd.Parameters.Add(new SqlParameter("@s3Result", s3Result));
                            cmd.Parameters.Add(new SqlParameter("@dynamoResult", dynamoResult));
                            cmd.ExecuteScalar();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorFormat("[FileMetaDB] SetValidateAWSResult exception:{0}", e.Message);
                    }
                }
            }

            _logger.ErrorFormat("[FileMetaDB] SetValidateAWSResult ERROR: DB has been reconnect {0} but fail", _reconnectCount);
            return false;
        }

        public Boolean DeleteLastMark()
        {
            _logger.Info("Delete lask mark in DB");
            for (int i = 0; i < _reconnectCount; i++)
            {
                using (SqlConnection conn = new SqlConnection(_dbConnStr))
                {
                    try
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand("spValidateDeleteLastMark", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.ExecuteScalar();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorFormat("[FileMetaDB] SetValidateAWSResult exception:{0}", e.Message);
                    }
                }
            }

            _logger.ErrorFormat("[FileMetaDB] SetValidateAWSResult ERROR: DB has been reconnect {0} but fail", _reconnectCount);
            return false;
        }
    }
}
