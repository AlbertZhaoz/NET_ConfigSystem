using System;
using System.IO;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace DotNetSystem_JsonProvider
{
    /// <summary>
    /// 邮件服务实现类
    /// </summary>
    public class MailService : IMailService
    {
        private readonly IOptionsSnapshot<MailKitRoot> options;


        public MailService(IOptionsSnapshot<MailKitRoot> options)
        {
            this.options = options;
        }

        #region 邮件发送基础服务API
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailBodyEntity">邮件基础信息</param>
        /// <param name="sendServerConfiguration">发件人基础信息</param>
        public SendResultEntity SendMail()
        {
            //初始化赋值
            var mailBodyEntity = options.Value.MailBodyEntity;
            var sendServerConfiguration = options.Value.SendServerConfigurationEntity;
            
            //为空判断
            if (mailBodyEntity == null)
            {
                throw new ArgumentNullException();
            }

            if (sendServerConfiguration == null)
            {
                throw new ArgumentNullException();
            }

            var sendResultEntity = new SendResultEntity();

            using (var client = new SmtpClient(new ProtocolLogger(CreateMailLog())))
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                Connection(mailBodyEntity, sendServerConfiguration, client, sendResultEntity);

                if (sendResultEntity.ResultStatus == false)
                {
                    return sendResultEntity;
                }

                SmtpClientBaseMessage(client);

                Authenticate(mailBodyEntity, sendServerConfiguration, client, sendResultEntity);

                if (sendResultEntity.ResultStatus == false)
                {
                    return sendResultEntity;
                }

                Send(mailBodyEntity, sendServerConfiguration, client, sendResultEntity);

                if (sendResultEntity.ResultStatus == false)
                {
                    return sendResultEntity;
                }
                client.Disconnect(true);
            }

            return sendResultEntity;
        }
        
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="mailBodyEntity">邮件内容</param>
        /// <param name="sendServerConfiguration">发送配置</param>
        /// <param name="client">客户端对象</param>
        /// <param name="sendResultEntity">发送结果</param>
        public static void Connection(MailBodyEntity mailBodyEntity,
            SendServerConfigurationEntity sendServerConfiguration,
            SmtpClient client, SendResultEntity sendResultEntity)
        {
            try
            {
                client.Connect(sendServerConfiguration.SmtpHost, sendServerConfiguration.SmtpPort);
            }
            catch (SmtpCommandException ex)
            {
                sendResultEntity.ResultInformation = $"尝试连接时出错:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
            catch (SmtpProtocolException ex)
            {
                sendResultEntity.ResultInformation = $"尝试连接时的协议错误:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
            catch (Exception ex)
            {
                sendResultEntity.ResultInformation = $"服务器连接错误:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
        }

        /// <summary>
        /// 账户认证
        /// </summary>
        /// <param name="mailBodyEntity">邮件内容</param>
        /// <param name="sendServerConfiguration">发送配置</param>
        /// <param name="client">客户端对象</param>
        /// <param name="sendResultEntity">发送结果</param>
        public static void Authenticate(MailBodyEntity mailBodyEntity,
            SendServerConfigurationEntity sendServerConfiguration,
            SmtpClient client, SendResultEntity sendResultEntity)
        {
            try
            {
                client.Authenticate(sendServerConfiguration.SenderAccount, sendServerConfiguration.SenderPassword);
            }
            catch (AuthenticationException ex)
            {
                sendResultEntity.ResultInformation = $"无效的用户名或密码:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
            catch (SmtpCommandException ex)
            {
                sendResultEntity.ResultInformation = $"尝试验证错误:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
            catch (SmtpProtocolException ex)
            {
                sendResultEntity.ResultInformation = $"尝试验证时的协议错误:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
            catch (Exception ex)
            {
                sendResultEntity.ResultInformation = $"账户认证错误:{0}" + ex.Message;
                sendResultEntity.ResultStatus = false;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailBodyEntity">邮件内容</param>
        /// <param name="sendServerConfiguration">发送配置</param>
        /// <param name="client">客户端对象</param>
        /// <param name="sendResultEntity">发送结果</param>
        public static void Send(MailBodyEntity mailBodyEntity, SendServerConfigurationEntity sendServerConfiguration,
            SmtpClient client, SendResultEntity sendResultEntity)
        {
            try
            {
                client.Send(MailMessage.AssemblyMailMessage(mailBodyEntity));
            }
            catch (SmtpCommandException ex)
            {
                switch (ex.ErrorCode)
                {
                    case SmtpErrorCode.RecipientNotAccepted:
                        sendResultEntity.ResultInformation = $"收件人未被接受:{ex.Message}";
                        break;
                    case SmtpErrorCode.SenderNotAccepted:
                        sendResultEntity.ResultInformation = $"发件人未被接受:{ex.Message}";
                        break;
                    case SmtpErrorCode.MessageNotAccepted:
                        sendResultEntity.ResultInformation = $"消息未被接受:{ex.Message}";
                        break;
                }

                sendResultEntity.ResultStatus = false;
            }
            catch (SmtpProtocolException ex)
            {
                sendResultEntity.ResultInformation = $"发送消息时的协议错误:{ex.Message}";
                sendResultEntity.ResultStatus = false;
            }
            catch (Exception ex)
            {
                sendResultEntity.ResultInformation = $"邮件接收失败:{ex.Message}";
                sendResultEntity.ResultStatus = false;
            }
        }

        /// <summary>
        /// 获取SMTP基础信息
        /// </summary>
        /// <param name="client">客户端对象</param>
        /// <returns></returns>
        public static MailServerInformation SmtpClientBaseMessage(SmtpClient client)
        {
            var mailServerInformation = new MailServerInformation
            {
                Authentication = client.Capabilities.HasFlag(SmtpCapabilities.Authentication),
                BinaryMime = client.Capabilities.HasFlag(SmtpCapabilities.BinaryMime),
                Dsn = client.Capabilities.HasFlag(SmtpCapabilities.Dsn),
                EightBitMime = client.Capabilities.HasFlag(SmtpCapabilities.EightBitMime),
                Size = client.MaxSize
            };

            return mailServerInformation;
        }

        /// <summary>
        /// 创建邮件日志文件
        /// </summary>
        /// <returns></returns>
        public static string CreateMailLog()
        {
            var logDir = AppDomain.CurrentDomain.BaseDirectory + "DocumentLog/";
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            var logPath = logDir + Guid.NewGuid() + ".txt";
            if (File.Exists(logPath)) return logPath;
            var fs = File.Create(logPath);
            fs.Close();
            return logPath;
        }
        #endregion
        
        #region 接收服务
        /// <summary>
        /// 设置读取配置文件路径
        /// TODO:需要引入DI来解耦，将配置、日志、邮箱服务拆分开。
        /// </summary>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public static IConfigurationRoot ReceiveEmailInit(string configFileName)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            //此处必须将reloadOnChange设置为true，每次改变都要加载
            configurationBuilder.AddJsonFile(configFileName, false, true);
            var rootConfig = configurationBuilder.Build();
            return rootConfig;
        }

        /// <summary>
        /// 设置发件人信息
        /// </summary>
        /// <returns></returns>
        public static SendServerConfigurationEntity SetSendMessage()
        {
            var rootConfig = ReceiveEmailInit("mailconfig.json");
            var sendServerConfiguration = new SendServerConfigurationEntity
            {
                SmtpHost = rootConfig["SmtpServer"],
                SmtpPort = int.Parse(rootConfig["SmtpPort"]),
                IsSsl = Convert.ToBoolean(rootConfig["IsSsl"]),
                MailEncoding = rootConfig["MailEncoding"],
                SenderAccount = rootConfig["SenderAccount"],
                SenderPassword = rootConfig["SenderPassword"]
            };
            return sendServerConfiguration;
        }

        /// <summary>
        /// 接收邮件
        /// </summary>
        public void ReceiveEmail()
        {
            var sendServerConfiguration = SetSendMessage();

            if (sendServerConfiguration == null)
            {
                throw new ArgumentNullException();
            }

            using (var client = new ImapClient(new ProtocolLogger(CreateMailLog())))
            {
                client.Connect(sendServerConfiguration.SmtpHost, sendServerConfiguration.SmtpPort,
                    SecureSocketOptions.SslOnConnect);
                client.Authenticate(sendServerConfiguration.SenderAccount, sendServerConfiguration.SenderPassword);
                client.Inbox.Open(FolderAccess.ReadOnly);
                var uids = client.Inbox.Search(SearchQuery.All);
                foreach (var uid in uids)
                {
                    var message = client.Inbox.GetMessage(uid);
                    message.WriteTo($"{uid}.eml");
                }

                client.Disconnect(true);
            }
        }

        /// <summary>
        /// 下载邮件内容
        /// </summary>
        public void DownloadBodyParts()
        {
            var sendServerConfiguration = SetSendMessage();

            using (var client = new ImapClient())
            {
                client.Connect(sendServerConfiguration.SmtpHost, sendServerConfiguration.SmtpPort,
                    SecureSocketOptions.SslOnConnect);
                client.Authenticate(sendServerConfiguration.SenderAccount, sendServerConfiguration.SenderPassword);
                client.Inbox.Open(FolderAccess.ReadOnly);

                // 搜索Subject标题包含“MimeKit”或“MailKit”的邮件
                var query = SearchQuery.SubjectContains("MimeKit").Or(SearchQuery.SubjectContains("MailKit"));
                var uids = client.Inbox.Search(query);

                // 获取搜索结果的摘要信息（我们需要UID和BODYSTRUCTURE每条消息，以便我们可以提取文本正文和附件）
                var items = client.Inbox.Fetch(uids, MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure);

                foreach (var item in items)
                {
                    // 确定一个目录来保存内容
                    var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/MailBody",
                        item.UniqueId.ToString());

                    Directory.CreateDirectory(directory);

                    // IMessageSummary.TextBody是一个便利的属性，可以为我们找到“文本/纯文本”的正文部分
                    var bodyPart = item.TextBody;

                    // 下载'text / plain'正文部分
                    var body = (TextPart) client.Inbox.GetBodyPart(item.UniqueId, bodyPart);

                    // TextPart.Text是一个便利的属性，它解码内容并将结果转换为我们的字符串
                    var text = body.Text;

                    File.WriteAllText(Path.Combine(directory, "body.txt"), text);

                    // 现在遍历所有附件并将其保存到磁盘
                    foreach (var attachment in item.Attachments)
                    {
                        // 像我们对内容所做的那样下载附件
                        var entity = client.Inbox.GetBodyPart(item.UniqueId, attachment);

                        // 附件可以是message / rfc822部件或常规MIME部件
                        var messagePart = entity as MessagePart;
                        if (messagePart != null)
                        {
                            var rfc822 = messagePart;

                            var path = Path.Combine(directory, attachment.PartSpecifier + ".eml");

                            rfc822.Message.WriteTo(path);
                        }
                        else
                        {
                            var part = (MimePart) entity;

                            // 注意：这可能是空的，但大多数会指定一个文件名
                            var fileName = part.FileName;

                            var path = Path.Combine(directory, fileName);

                            // decode and save the content to a file
                            using (var stream = File.Create(path))
                                part.Content.DecodeTo(stream);
                        }
                    }
                }

                client.Disconnect(true);
            }
        }
        #endregion
    }
    #region 组装邮件消息
    /// <summary>
    /// 邮件信息
    /// </summary>
    public static class MailMessage
    {
        /// <summary>
        /// 组装邮件文本/附件邮件信息
        /// </summary>
        /// <param name="mailBodyEntity">邮件消息实体</param>
        /// <returns></returns>
        public static MimeMessage AssemblyMailMessage(MailBodyEntity mailBodyEntity)
        {
            if (mailBodyEntity == null)
            {
                throw new ArgumentNullException(nameof(mailBodyEntity));
            }

            var message = new MimeMessage();

            //设置邮件基本信息
            SetMailBaseMessage(message, mailBodyEntity);

            var multipart = new Multipart("mixed");

            //插入文本消息
            if (string.IsNullOrEmpty(mailBodyEntity.MailTextBody) == false)
            {
                var alternative = new MultipartAlternative
                {
                    AssemblyMailTextMessage(mailBodyEntity.MailTextBody, mailBodyEntity.MailBodyType)
                 };

                multipart.Add(alternative);
            }

            //插入附件
            if (mailBodyEntity.MailFilePath != null && File.Exists(mailBodyEntity.MailFilePath) == true)
            {
                var mimePart = AssemblyMailAttachmentMessage(mailBodyEntity.MailFileType, mailBodyEntity.MailFileSubType,
                     mailBodyEntity.MailFilePath);

                multipart.Add(mimePart);
            }

            //组合邮件内容
            message.Body = multipart;

            return message;
        }

        /// <summary>
        /// 设置邮件基础信息
        /// </summary>
        /// <param name="minMessage"></param>
        /// <param name="mailBodyEntity"></param>
        /// <returns></returns>
        public static MimeMessage SetMailBaseMessage(MimeMessage minMessage, MailBodyEntity mailBodyEntity)
        {
            if (minMessage == null)
            {
                throw new ArgumentNullException();
            }

            if (mailBodyEntity == null)
            {
                throw new ArgumentNullException();
            }

            //插入发件人
            minMessage.From.Add(new MailboxAddress(mailBodyEntity.Sender, mailBodyEntity.SenderAddress));

            //插入收件人
            foreach (var recipients in mailBodyEntity.Recipients)
            {
                minMessage.To.Add(new MailboxAddress(string.Empty,recipients));
            }

            //插入抄送人
            if (mailBodyEntity.Cc != null)
            {
                foreach (var cC in mailBodyEntity.Cc)
                {
                    minMessage.Cc.Add(new MailboxAddress(string.Empty,cC));
                }
            }
            
            //插入主题
            minMessage.Subject = mailBodyEntity.Subject;

            return minMessage;
        }

        /// <summary>
        /// 组装邮件文本信息
        /// </summary>
        /// <param name="mailBody">邮件文本内容</param>
        /// <param name="textPartType">邮件文本类型(plain,html,rtf,xml)</param>
        /// <returns></returns>
        public static TextPart AssemblyMailTextMessage(string mailBody, string textPartType)
        {
            if (string.IsNullOrEmpty(mailBody))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(textPartType))
            {
                throw new ArgumentNullException();
            }

            var textBody = new TextPart(textPartType)
            {
                Text = mailBody
            };

            return textBody;
        }

        /// <summary>
        /// 组装邮件附件信息
        /// </summary>
        /// <param name="fileAttachmentType">附件类型(image,application)</param>
        /// <param name="fileAttachmentSubType">附件子类型 </param>
        /// <param name="fileAttachmentPath">附件路径</param>
        /// <returns></returns>
        public static MimePart AssemblyMailAttachmentMessage(string fileAttachmentType, string fileAttachmentSubType, string fileAttachmentPath)
        {
            if (string.IsNullOrEmpty(fileAttachmentSubType))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(fileAttachmentType))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(fileAttachmentPath))
            {
                throw new ArgumentNullException();
            }

            var attachment = new MimePart(fileAttachmentType, fileAttachmentSubType)
            {
                Content = new MimeContent(File.OpenRead(fileAttachmentPath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(fileAttachmentPath)
            };

            return attachment;
        }

    }
    #endregion
    
}