using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace z_Upload
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        crc16 crc_file = new crc16();
        string filePath, fileName,fileZmcPath;
        UInt16 fileSize;
        FileStream fileStream;
        private void ouputFile_click(object sender, RoutedEventArgs e)
        {

         }
        string outPath;
        private void btn_convert_click(object sender, RoutedEventArgs e)
        {
            //新建文件
           
            outPath = System.IO.Path.GetDirectoryName(filePath);
            fileZmcPath = outPath + "\\" + fileName + "_version" + app.Version + ".zmc";
            string jsonPath = outPath + "\\" + "zjson.cfg";
            FileStream jsonFile = new FileStream(jsonPath, FileMode.Create);
            BinaryWriter BinaryWrite = new BinaryWriter(jsonFile);
            BinaryReader binaryReader = new BinaryReader(jsonFile);
            
            textBox_outFileName.Text = fileZmcPath;
            //json    
            string json = JsonConvert.SerializeObject(app);
            byte[] jsonFileBuf = System.Text.Encoding.Default.GetBytes(json);
            byte[] jsonFileBufLenth = new byte[2];
            jsonFileBufLenth[0] = (byte)((UInt16)json.Length >> 8);
            jsonFileBufLenth[1] = (byte)((UInt16)json.Length);
            byte[] final_param = jsonFileBufLenth.Concat(jsonFileBuf).ToArray();  
            BinaryWrite.Write(final_param);

            BinaryWrite.Close();
            jsonFile.Close();

            string[] filePaths = { jsonPath, filePath }; 
            CombineFiles(filePaths, fileZmcPath);//合并文件
        }
        VersionApp app = new VersionApp();
        private void btn_fileSelect_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog(); 
            ofd.DefaultExt = ".bin";
            ofd.Filter = "zjk@MaiCe 固件|*.bin";
            if (ofd.ShowDialog() == true)
            {               
                filePath = ofd.FileName;            
                fileStream = new FileStream(@filePath, FileMode.Open, FileAccess.Read);
                BinaryReader fileRead = new BinaryReader(fileStream);
                byte[] bin_buff = fileRead.ReadBytes((int) fileStream.Length);
                app.Filesize = fileSize = (UInt16)fileStream.Length;
                app.Version = textBox_fileVersion.Text;
                app.TimeCreate = DateTime.Now.ToLocalTime().ToString();
                textBlock_fileSize.Text = fileSize.ToString();
                app.Name = fileName = textBox_fileName.Text = System.IO.Path.GetFileNameWithoutExtension(filePath);  //获取无后缀名称
                UInt16 crc = crc_file.crc16_modbus(bin_buff);
                app.Crc16Modulbus = crc;
                UInt16 crc_test =  Convert.ToUInt16(app.Crc16Modulbus);
                //bin文件加入
                fileStream.Close();
               
            }
        }


        public class VersionApp
        {
            public string Name { set; get; }
            public string Version { set; get; }
            public int Filesize { set; get; }
            public string TimeCreate { set; get; }
            public int Crc16Modulbus { set; get; }
        }

        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="filePaths">要合并的文件列表</param>
        /// <param name="combineFile">合并后的文件路径带文件名</param>
       public void CombineFiles(string[] filePaths, string combineFile)
        {
            FileStream CombineStream = new FileStream(combineFile, FileMode.OpenOrCreate);
            BinaryWriter CombineWriter = new BinaryWriter(CombineStream);
            foreach (string file in filePaths)
             {
                FileStream fileStream = new FileStream(file, FileMode.Open);
                BinaryReader fileReader = new BinaryReader(fileStream);
                byte[] TempBytes = fileReader.ReadBytes((int)fileStream.Length);
                CombineWriter.Write(TempBytes);
              }

            CombineStream.Close();
            CombineWriter.Close();
        }

    }
}
