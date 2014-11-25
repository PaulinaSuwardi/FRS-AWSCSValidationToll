using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace FRS_AWSCSSync
{
    public class AppConfigClass
    {
        public static string AWSAccessKey
        {
            get
            {
                return ConfigurationManager.AppSettings["AWSAccessKey"];
            }
        }

        public static string AWSSecretKey
        {
            get
            {
                return ConfigurationManager.AppSettings["AWSSecretKey"];
            }
        }

        public static string DBConnectionStr
        {
            get
            {
                return ConfigurationManager.AppSettings["DBConnectionStr"];
            }
        }

        public static int DBReconnectCount
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["DBReconnectCount"]);
            }
        }

        public static int NumberOfDataPerDBQuery
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfDataPerDBQuery"]);
            }
        }

        public static int DBReaderThreadWaitInSec
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["DBReaderThreadWaitInSec"]);
            }
        }

        public static int DBReaderThreadCount
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["DBReaderThreadCount"]);
            }
        }

        public static int AwsCheckerThreadCount
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["AwsCheckerThreadCount"]);
            }
        }

        public static string DBReaderName
        {
            get
            {
                return ConfigurationManager.AppSettings["DBReaderName"];
            }
        }

        public static bool DeleteLastMark
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings["DeleteLastMark"]);
            }
        }

    }
}
