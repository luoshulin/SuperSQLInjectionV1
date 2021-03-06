﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using tools;
using model;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows.Forms;
using SuperSQLInjection.model;
using SuperSQLInjection;
using SuperSQLInjection.tools;

namespace tools
{
    class Tools
    {
        public const String httpLogPath = "logs/http/";

        public static long currentMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        public static bool ThreadPoolIsEnd()
        {
            int workerThreads = 0;
            int maxWordThreads = 0;
            //int 
            int compleThreads = 0;
            ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
            ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);

            if (maxWordThreads == workerThreads)
            {
                return true;
            }
            else {
                return false;
            }
        }

        public static void SysLog(String log)
        {
            FileTool.AppendLogToFile("logs/" + DateTime.Now.ToLongDateString() + ".log.txt", log + "----" + DateTime.Now);
        }

        public static String RandStr(int len)
        {
            StringBuilder str = new StringBuilder();
            Random rd = new Random();
            for (int i=0;i<len;i++) {
                char c=(char)rd.Next(65, 91);
                str.Append(c);
            }
            return str.ToString();
        }

        public static String fomartTime(String time)
        {
            try
            {
                DateTime dt = Convert.ToDateTime(time);
                String newtime = dt.ToLocalTime().ToString();
                return newtime;
            }
            catch (Exception e)
            {
                SysLog(e.Message);
            }
            return time;

        }

        /// <summary>
        /// 二分法取较大整数，用于盲注判断
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int getLargeNum(int start,int end){
  
            int sum=start+end;
            if (sum == 1) {
                return 0;
            }
            if (sum % 2 == 0)
            {
                return sum / 2;
             }
             else {
                    return sum / 2;
             }
        
        }


        public static String unHexByUnicode(int unicode,String encoding){

            int c = Tools.UnicodeInt2UTF8Int(unicode);
            return Tools.unHex(Convert.ToString(c, 16), encoding);
        
        }

        public static String hexToRaw(string str,String encoding){
            if (str.Length % 2 == 0)
            {
                byte[] b = new byte[str.Length / 2];
                int j = 0;  
                for (int i = 0; i < str.Length; i += 2){
                    byte by = Convert.ToByte(str.Substring(i, 2), 16);//取两个字符，转换成对应的字节
                    b[j] = by;
                    j++;
                }
                return Encoding.GetEncoding(encoding).GetString(b);
            }
            else{
                throw new Exception("不能将该字符串转换成String类型!");
            }
        }

        public static void sysHTTPLog(String index ,ServerInfo server)
        {
            FileTool.AppendLogToFile(httpLogPath + index + "-request.txt", server.request);
            FileTool.AppendLogToFile(httpLogPath + index + "-response.txt", server.header + "\r\n\r\n" + server.body);
        }

        public static void delHTTPLog()
        {
            try
            {
                DirectoryInfo din = new DirectoryInfo(httpLogPath);
                FileInfo[] files = din.GetFiles();
                foreach (FileInfo f in files)
                {
                    f.Delete();
                }
            }
            catch (Exception re)
            {
                Tools.SysLog("删除HTTP日志发生错误！" + re.Message);
            }
        }


        /// <summary>
        /// Hex解码
        /// </summary>
        /// <param name="hex">Hex编码</param>
        /// <param name="charset">字符编码</param>
        /// <returns></returns>
        public static string unHex(string hex, string charset){
            if (hex == null)throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("\n", "");
            hex = hex.Replace("\\", "");
            hex = hex.Replace(" ", ""); 
            if (hex.Length % 2 != 0){
                hex += "20";//空格
            }
            // 需要将 hex 转换成 byte 数组。              
            byte[] bytes = new byte[hex.Length / 2];            
            for (int i = 0; i < bytes.Length; i++){ 
            try{
                // 每两个字符是一个 byte。
                bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                System.Globalization.NumberStyles.HexNumber);
            } catch{
                // Rethrow an exception with custom message.
                SysLog("unHex解码错误---hex is not a valid hex number!");
            }
            }
            Encoding chs = Encoding.GetEncoding(charset);
            return chs.GetString(bytes);
        }
        /// <summary>
        /// 将数组转换成字符串
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static String convertToString(String[] strs){

            StringBuilder sb = new StringBuilder();
            foreach(String s in strs){
                sb.Append(s);
            }
            return sb.ToString();
        
        }

        /// <summary>
        /// 将字符串转换成数字，错误返回0
        /// </summary>
        /// <param name="strs">字符串</param>
        /// <returns></returns>
        public static int convertToInt(String str)
        {

            try
            {
                return int.Parse(str);
            }
            catch (Exception e) {
                Tools.SysLog("info:-"+e.Message);
            }
            return 0;

        }
        /// <summary>
        /// 将16进制转换成10进制
        /// </summary>
        /// <param name="str">16进制字符串</param>
        /// <returns></returns>
        public static int convertToIntBy16(String str)
        {
          try
            {
                return Convert.ToInt32(str,16);
            }
            catch (Exception e)
            {
                
            }
            return 0;

        }

        public static int findKeyCount(String str,String key)
        {
            int count = 0;
            try
            {
                if (!String.IsNullOrEmpty(str))
                {
                    int index = 0;

                    while (index != -1)
                    {
                        index = str.IndexOf(key, index + 1);
                        if (index != -1)
                        {
                            count++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Tools.SysLog("findKeyCount发生异常！"+e.Message);
            }
            return count;

        }

        public static Boolean checkEmpty(String str) {

            if (str != null && str.Length > 0)
            {
                return false;
            }
            else {
                return true;
            }
        }

        public static String StringArrayToString(String[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in array) {

                if (s != null)
                {

                    sb.Append(s);
                }
                else {

                    sb.Append("_");
                }
            
            }
            return sb.ToString();
        }
        /// <summary>
        /// 判断页面注入true或false
        /// </summary>
        /// <param name="server">服务器响应对象ServerInfo</param>
        /// <param name="isUseCode">是否使用状态码判断</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public static Boolean isTrue(ServerInfo server,String key,Boolean reverKey,KeyType keyType)
        {
            switch (keyType) {
                case KeyType.Key:

                    //用关键字判断
                    if (server.body.Length > 0 && server.body.IndexOf(key) != -1)
                    {
                        if (reverKey)
                        {
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        if (reverKey)
                        {
                            return true;
                        }
                        return false;
                    }

                case KeyType.Code:
                    //用状态码判断
                    if (server.code > 0 && key.Equals(server.code + ""))
                    {
                        if (reverKey)
                        {
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        if (reverKey)
                        {
                            return true;
                        }
                        return false;
                    }
                case KeyType.Length:
                    //用长度判断
                    if (key.Equals(server.length.ToString()))
                    {
                        if (reverKey)
                        {
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        if (reverKey)
                        {
                            return true;
                        }
                        return false;
                    }

                case KeyType.Time:
                    int time = Tools.convertToInt(key);
                    if (server.runTime > time*1000)
                    {
                        if (reverKey)
                        {
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        if (reverKey)
                        {
                            return true;
                        }
                        return false;
                    }

            }
            return false;
           
        }
       
        public static String strToHex(String str,String encode)
        {
            try
            {
                
                StringBuilder sb = new StringBuilder();//  存储转换后的编码
                Byte[] strByte=Encoding.GetEncoding(encode).GetBytes(str);
                foreach (Byte s in strByte)
                {
                    sb.Append(s.ToString("x").PadLeft(2, '0'));
                }
                return "0x" + sb.ToString();


            }
            catch (Exception e)
            {
                Tools.SysLog("hex转换错误，传递str:" + str + ",encode:" + encode + "！错误消息：" + e.Message);
            }
            return "";
        }
        public static int UnicodeInt2UTF8Int(int UnicodeInt)
        {
            if (UnicodeInt < 128)
            {
                return UnicodeInt;
            }
            int num = UnicodeInt >> 12 & 15;
            int num2 = UnicodeInt >> 6 & 63;
            int num3 = UnicodeInt & 63;
            return (num + 224 << 16) + (num2 + 128 << 8) + (num3 + 128);
        }

        public static int UTF8Int2UnicodeInt(int UTF8Int)
        {
            if (UTF8Int < 128)
            {
                return UTF8Int;
            }
            int num = UTF8Int >> 16 & 15;
            int num2 = UTF8Int >> 8 & 63;
            int num3 = UTF8Int & 63;
            return (num << 12) + (num2 << 6) + num3;
        }

        public static String randIP()
        {
            Random rd = new Random();

            String ip = rd.Next(1, 255) + "." + rd.Next(1, 255) + "." + rd.Next(1, 255) + "." + rd.Next(1, 255);

            return ip;
        }

        public static String stringToAscii(String str)
        {
            char[] cstr = str.ToCharArray();
            StringBuilder sb = new StringBuilder();
            foreach (char c in cstr) {
                sb.Append(Convert.ToInt32(c) + " ");
            }
            if (sb.Length > 1) {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public static String asciiToString(String str)
        {
            try
            {
                String[] sstr = str.Split(' ');
                StringBuilder sb = new StringBuilder();
                foreach (String c in sstr)
                {
                    sb.Append(((char)(int.Parse(c))));
                }
                return sb.ToString();
            }
            catch (Exception e) {

                Tools.SysLog("waring:asciiToString发生错误，"+e.Message);
            
            }
            return "";
        }

        public static decimal getLike(String body1, String body2)
        {

            String[] keys1 = Regex.Split(body1, "[^\\u0080-\\uFFFF\\w\\-\\d]+");
            String[] keys2 = Regex.Split(body2, "[^\\u0080-\\uFFFF\\w\\-\\d]+");

            HashSet<String> hash1 = new HashSet<String>();
            HashSet<String> hash2 = new HashSet<String>();
            foreach (String key in keys1)
            {
                if (!hash1.Contains(key))
                {
                    hash1.Add(key);
                }
            }
            foreach (String key in keys2)
            {
                if (!hash2.Contains(key))
                {
                    hash2.Add(key);
                }
            }
            int count = 0;
            foreach (String key in hash2)
            {
                if (hash1.Contains(key))
                {
                    count++;
                }
            }
            decimal p = 0;
            if (hash1.Count > 0)
            {
                decimal cc = (decimal)((float)count * 100 / hash1.Count);
                p = decimal.Round(cc, 2);
            }
            return p;
        }

        public static String findKeyByStr(String trueString, String falseString, String oldString)
        {
            try
            {   //以时间判断
                String key = "";

               String[] Keys = Regex.Split(oldString, "[^\\u0080-\\uFFFF\\w\\d]+");
               Array.Sort(Keys, new StringLengthComparer());
               foreach (String ckey in Keys) {
                    if (falseString.IndexOf(ckey) == -1 && trueString.IndexOf(ckey) >= 0) {
                        return ckey;
                    }
               }
                for (int length = 5; length >= 1; length--)
                {
                        for (int i = 0; i < trueString.Length - length; i++)
                        {
                            if (trueString.Length <= length && !trueString.Equals(falseString))
                            {
                                return trueString;
                            }
                            String tempKey = trueString.Substring(i, length);
                            if (falseString.IndexOf(tempKey) == -1&& oldString.IndexOf(key)>=0)
                            {
                                key = tempKey;
                                Regex regex = new Regex("[\\S]+");
                                //非制表符，返回结果，否则继续查看是否还有其他关键词
                                if (regex.IsMatch(key)) {
                                    return key;
                                }
                            }

                        }
                    
                }
                return key;

            }
            catch (Exception e)
            {

                Tools.SysLog("warin：查找注入关键字发生错误，" + e.Message);

            }
            return "";
        }
        public static int findKeyByCode(int trueCode, int falseCode)
        {
            if (trueCode != falseCode) {
                return trueCode;
            }
            return 0;

        }

        public static int findKeyByTime(int trueTime, int falseTime,int maxTime)
        {
            if (trueTime > maxTime&&falseTime<maxTime) {
                return maxTime;
            }
   
                return 0;
        }

        public static String clearURLParams(String url)
        {
                int index = url.IndexOf("?");
                if (index > 0)
                {
                    return url.Substring(0,index);

                }
                else {

                    return url;
                }
        }

        public static String getCurrentPath(String url)
        {
            int index =url.LastIndexOf("/");

            if (index != -1)
            {
                return url.Substring(0,index)+"/";
            }
            else {
                return "";
            }
        }

        public static String getRootDomain(String domain)
        {
            int index = domain.LastIndexOf(".");

            if (index>0)
            {
                int index2 = domain.LastIndexOf(".", index - 1);
                if (index2 != -1)
                {
                    return domain.Substring(index2+1);
                }
               
            }
            return domain;
        }

        public static String md5_16(String str){
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            String t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(str)), 4, 8);
            t2 = t2.Replace("-", "");
            t2 = t2.ToLower();
            return t2;
        }
        public static String md5_32(String str)
        {
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            String pwd = "";
            for (int i = 0; i < s.Length; i++)
            {
                //将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                pwd = pwd + s[i].ToString("X");
            }
            return pwd;

        }
        public static bool isExistsNode(TreeNodeCollection tvws, String key)
        {

            foreach (TreeNode tn in tvws)
            {

                if (tn.Text.Equals(key))
                {
                    return true;
                }
            }

            return false;

        }


        public static String changeRequestMethod(String datapack)
        {
            if (datapack.StartsWith("GET"))
            {
                int pl = datapack.IndexOf("?");
                if (pl != -1) {
                    int el = datapack.IndexOf(" ",pl);
                    if (el != -1) {

                       String cparams= datapack.Substring(pl+1,el-pl-1);
                       datapack = datapack.Replace("?"+ cparams,"");
                       int sl= datapack.IndexOf("\r\n");
                       datapack= datapack.Insert(sl, "\r\nContent-Type: application/x-www-form-urlencoded\r\nContent-Length: 0");
                       int ssl = datapack.IndexOf("\r\n\r\n");
                        if (!datapack.EndsWith("\r\n\r\n")) {

                            datapack += "\r\n\r\n";
                        }
                       datapack+=cparams;

                       int me = datapack.IndexOf(" ");
                        if (me != -1) {

                            datapack = "POST" + datapack.Substring(me, datapack.Length - me);
                        }

                       return datapack;
                    }
                }
            }

            else if (datapack.StartsWith("POST"))
            {
                int ssl = datapack.IndexOf("\r\n\r\n");

                if (ssl != -1) {

                  
                    String cparams = datapack.Substring(ssl+4,datapack.Length- ssl - 4);
                    datapack = datapack.Substring(0, ssl+1);
                    int cys = datapack.IndexOf("Content-Type");
                    int cye = datapack.IndexOf("\r\n",cys);

                    if (cye > cys) {
                        datapack=datapack.Remove(cys, cye - cys+2);
                    }
                    int cls = datapack.IndexOf("Content-Length");
                    int cle = datapack.IndexOf("\r\n", cls+1);
                    if (cle > cls)
                    {
                        datapack = datapack.Remove(cls, cle - cls+2);
                    }

                    int hl = datapack.IndexOf(" HTTP");
                    if (hl != -1) {

                        datapack = datapack.Insert(hl, "?"+cparams);
                    }
                   
                    int me = datapack.IndexOf(" ");

                    if (me != -1)
                    {

                        datapack = "GET" + datapack.Substring(me, datapack.Length - me);
                    }
                }
            }
            
            return datapack;

        }
    }
}
