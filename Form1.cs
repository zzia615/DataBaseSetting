using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace DatabaseSetting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            comboBox1.Text = "Windows身份验证";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetFile();
        }

        string fileName = "";
        XmlDocument document = new XmlDocument();
        void GetFile()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            FileInfo[] files = directory.GetFiles("*.config");
            if (File.Exists("Web.config"))
            {
                fileName = "Web.config";
            }
            else if (File.Exists("App.config"))
            {
                fileName = "App.config";
            }
            else
            {

                if (files.Length <= 0)
                {

                    MessageBox.Show("未找到数据库配置文件");
                    this.Close();
                    return;
                }
                else
                {
                    fileName = files[0].FullName;
                }
            }

            document.Load(fileName);

            var nodes = document.SelectNodes("/configuration/connectionStrings/add");

            var node = nodes[0];

            string conStr = node.Attributes["connectionString"].InnerText;
            //去除最后一个分号
            conStr = conStr.TrimEnd(new char[] { ';' });

            var tmp = conStr.Split(';');

            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox1.Text = tmp[0].Split('=')[1];
            textBox2.Text = tmp[1].Split('=')[1];
            if (tmp.Length == 4)
            {
                textBox3.Enabled = true;
                textBox4.Enabled = true;
                textBox3.Text = tmp[2].Split('=')[1];
                textBox4.Text = tmp[3].Split('=')[1];
                comboBox1.Text = "SqlServer身份验证";
            }
            if (tmp.Length == 3)
            {
                comboBox1.Text = "Windows身份验证";
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Windows身份验证")
            {
                textBox3.Enabled = false;
                textBox4.Enabled = false;
            }
            else
            {
                textBox3.Enabled = true;
                textBox4.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var nodes = document.SelectNodes("/configuration/connectionStrings/add");
                var node = nodes[0];
                string providerName = "System.Data.SqlClient";
                try
                {
                    providerName = node.Attributes["providerName"].InnerText;

                }
                catch
                {
                    var attr = document.CreateAttribute("providerName");
                    attr.InnerText = providerName;
                    node.Attributes.Append(attr);
                }

                var con = System.Data.Common.DbProviderFactories.GetFactory(providerName).CreateConnection();
                string conStr = "";
                if (comboBox1.Text == "Windows身份验证")
                {
                    conStr = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=true;"
                        , textBox1.Text, textBox2.Text);
                }
                else
                {
                    conStr = string.Format("Data Source={0};Initial Catalog={1};UID={2};PWD={3};"
                        , textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text);
                }
                con.ConnectionString = conStr;
                con.Open();

                node.Attributes["connectionString"].InnerText = conStr;
                document.Save(fileName);
                MessageBox.Show("保存成功");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
