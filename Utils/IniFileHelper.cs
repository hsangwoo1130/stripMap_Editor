using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace StripMapEditor.Utils
{
    /// INI 파일 읽기/쓰기를 위한 헬퍼 클래스
    public class IniFileHelper
    {
        private string _filePath;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// INI 파일 경로를 지정하여 초기화 (절대 경로 필수)
        /// </summary>
        public IniFileHelper(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
                throw new ArgumentException("IniFileHelper는 절대 경로가 필요합니다.", nameof(filePath));

            _filePath = filePath;
        }

        /// <summary>
        /// INI 파일에서 값 읽기
        /// </summary>
        public string Read(string section, string key, string defaultValue = "")
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue, temp, 255, _filePath);
            return temp.ToString();
        }

        /// <summary>
        /// INI 파일에 값 쓰기
        /// </summary>
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _filePath);
        }

    }
}
