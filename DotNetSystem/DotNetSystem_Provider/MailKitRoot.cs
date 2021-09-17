using System.Collections.Generic;

namespace DotNetSystem_JsonProvider
{
     #region 邮件实体类

    /// <summary>
    /// 实体类根节点
    /// </summary>
    public class MailKitRoot
    {
        public string name { get; set; }
        public MailBodyEntity MailBodyEntity { get; set; }
        public SendServerConfigurationEntity SendServerConfigurationEntity { get; set; }
    }
    
    /// <summary>
    /// 邮件内容实体
    /// </summary>
    public class MailBodyEntity
    {
        /// <summary>
        /// 邮件文本内容
        /// </summary>
        public string MailTextBody { get; set; }

        /// <summary>
        /// 邮件内容类型
        /// </summary>
        public string MailBodyType { get; set; }

        /// <summary>
        /// 邮件附件文件类型
        /// </summary>
        public string MailFileType { get; set; }

        /// <summary>
        /// 邮件附件文件子类型
        /// </summary>
        public string MailFileSubType { get; set; }

        /// <summary>
        /// 邮件附件文件路径
        /// </summary>
        public string MailFilePath { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public List<string> Recipients { get; set; }

        /// <summary>
        /// 抄送
        /// </summary>
        public List<string> Cc { get; set; }

        /// <summary>
        /// 发件人
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// 发件人地址
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 邮件内容
        /// </summary>
        public string Body { get; set; }
    }

    /// <summary>
    /// 邮件服务器基础信息
    /// </summary>
    public class MailServerInformation
    {
        /// <summary>
        /// SMTP服务器支持SASL机制类型
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// SMTP服务器对消息的大小
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// SMTP服务器支持传递状态通知
        /// </summary>
        public bool Dsn { get; set; }

        /// <summary>
        /// SMTP服务器支持Content-Transfer-Encoding
        /// </summary>
        public bool EightBitMime { get; set; }

        /// <summary>
        /// SMTP服务器支持Content-Transfer-Encoding
        /// </summary>
        public bool BinaryMime { get; set; }

        /// <summary>
        /// SMTP服务器在消息头中支持UTF-8
        /// </summary>
        public string UTF8 { get; set; }
    }

    /// <summary>
    /// 邮件发送结果
    /// </summary>
    public class SendResultEntity
    {
        /// <summary>
        /// 结果信息
        /// </summary>
        public string ResultInformation { get; set; } = "发送成功！";

        /// <summary>
        /// 结果状态
        /// </summary>
        public bool ResultStatus { get; set; } = true;
    }

    /// <summary>
    /// 邮件发送服务器配置
    /// </summary>
    public class SendServerConfigurationEntity
    {
        /// <summary>
        /// 邮箱SMTP服务器地址
        /// </summary>
        public string SmtpHost { get; set; }

        /// <summary>
        /// 邮箱SMTP服务器端口
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// 是否启用IsSsl
        /// </summary>
        public bool IsSsl { get; set; }

        /// <summary>
        /// 邮件编码
        /// </summary>
        public string MailEncoding { get; set; }

        /// <summary>
        /// 发件人账号
        /// </summary>
        public string SenderAccount { get; set; }

        /// <summary>
        /// 发件人密码
        /// </summary>
        public string SenderPassword { get; set; }

    }
    

    #endregion
}