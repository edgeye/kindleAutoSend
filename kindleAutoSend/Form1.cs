using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kindleAutoSend
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private MailMessage mailMessage;
        private SmtpClient smtpClient;
        private string password;//发件人密码 

        private void btnchoosefile_Click(object sender, EventArgs e)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            //openFileDialog.Filter = "文本文件|*.*|C#文件|*.cs|所有文件|*.*";
            //openFileDialog.RestoreDirectory = true;
            //openFileDialog.FilterIndex = 1;
            //if (openFileDialog.ShowDialog() == DialogResult.OK)
            //{


            //    //this.textBox1.Text = openfiled;

            //}
            this.txtBooks.Text = this.txtBooks.Text == "" ? SelectPathFile() : this.txtBooks.Text + "," + SelectPathFile();

        }

        public void SendMail(string To, string From, string Body, string Title, string Password)
        {
            mailMessage = new MailMessage();
            mailMessage.To.Add(To);
            mailMessage.From = new System.Net.Mail.MailAddress(From);
            mailMessage.Subject = Title;
            mailMessage.Body = Body;
            mailMessage.IsBodyHtml = true;
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.Priority = System.Net.Mail.MailPriority.Normal;
            this.password = Password;
        }
        /// <summary>  
        /// 添加附件  
        /// </summary>  
        public void Attachments(string Path)
        {
            string[] path = Path.Split(',');
            Attachment data;
            ContentDisposition disposition;
            for (int i = 0; i < path.Length; i++)
            {
                data = new Attachment(path[i], MediaTypeNames.Application.Octet);//实例化附件  
                //disposition = data.ContentDisposition;
                //disposition.CreationDate = System.IO.File.GetCreationTime(path[i]);//获取附件的创建日期  
                //disposition.ModificationDate = System.IO.File.GetLastWriteTime(path[i]);//获取附件的修改日期  
                //disposition.ReadDate = System.IO.File.GetLastAccessTime(path[i]);//获取附件的读取日期  
                mailMessage.Attachments.Add(data);//添加到附件中  
            }
        }
        /// <summary>  
        /// 异步发送邮件  
        /// </summary>  
        /// <param name="CompletedMethod"></param>  
        public void SendAsync(SendCompletedEventHandler CompletedMethod)
        {
            if (mailMessage != null)
            {
                smtpClient = new SmtpClient();
                smtpClient.Credentials = new System.Net.NetworkCredential(mailMessage.From.Address, password);//设置发件人身份的票据  
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Host = "smtp." + mailMessage.From.Host;
                smtpClient.SendCompleted += new SendCompletedEventHandler(CompletedMethod);//注册异步发送邮件完成时的事件  
                smtpClient.SendAsync(mailMessage, mailMessage.Body);
            }
        }
        /// <summary>  
        /// 发送邮件  
        /// </summary>  


        private void btnSend_Click(object sender, EventArgs e)
        {
            fThread = new Thread(new ThreadStart(SleepT));//开辟一个新的线程  
            fThread.Start();
            this.btnSend.Enabled = false;



            string to = txtReMail.Text;
            string from = txtSendMail.Text;
            string body = txtSendContent.Text;
            string title = txtTitle.Text;
            string password = txtSendMailPsd.Text;
            SendMail(to, from, body, title, password);
            string path = this.txtBooks.Text;
            if (path != null && !path.Equals(""))
            {
                Attachments(path);
            }


            if (mailMessage != null)
            {
                smtpClient = new SmtpClient();
                smtpClient.Credentials = new System.Net.NetworkCredential(mailMessage.From.Address, password);//设置发件人身份的票据  
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Host = "smtp." + mailMessage.From.Host;
                smtpClient.Send(mailMessage);
            }

            
        }

        // 选择文件：
        private string SelectPathFile()
        {
            string path = string.Empty;
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Files (*.*)|*.*"//如果需要筛选txt文件（"Files (*.txt)|*.txt"）
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }
            return path;
        }
        // 选择路径
        private string SelectPath()
        {
            string path = string.Empty;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
            }
            return path;
        }


        //更新进度列表  
        private delegate void SetPos(int ipos);

        private void SetTextMessage(int ipos)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(SetTextMessage);
                this.Invoke(setpos, new object[] { ipos });
            }
            else
            {
                this.label7.Text = ipos.ToString() + "%";
                this.progressBar1.Value = Convert.ToInt32(ipos);
            }
        }

        public Thread fThread;

        private void SleepT()
        {
            for (int i = 0; i <= 500; i++)
            {
                System.Threading.Thread.Sleep(10); //线程休眠，控制进度条的速度。  
                SetTextMessage(100 * i / 500);

            }
            Control.CheckForIllegalCrossThreadCalls = false;
            if (progressBar1.Value == 100)
            {
                if (MessageBox.Show("已完成！", "提示", MessageBoxButtons.OK) == DialogResult.OK)
                {
                    progressBar1.Value = 0;
                    this.label7.Text = "0%";
                    this.btnSend.Enabled = true;
                }
            }

        }

        //不能在form主线程以外的线程中调用 控件，所以必须用委托，暂时用 Control.CheckForIllegalCrossThreadCalls = false;这个方法越过检查
        ////主线程
        //public delegate void UpdateMessage(string mes);
        //public void UpdatePortMessage(string mes)
        //{
        //    this.txtMessage.AppendText(string.Format("Connected {0}. succeed", mes) + "\r\n");
        //    this.txtClientIP.Text = mes;
        //}


        //// 线程1
        //UpdateMessage upPort = new UpdateMessage(UpdatePortMessage);
        //                this.BeginInvoke(upPort, point);
    }
}
