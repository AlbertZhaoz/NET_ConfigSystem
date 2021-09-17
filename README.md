# .NET配置系统

拖了很久的配置系统总结终于于210914完成写作，本文主要介绍最新的.NET5（.NET6 preview也支持）下的配置系统。该文章默认读者已掌握基础的.NET开发及DI（Dependency Injection)的概念和简单使用。本文长文预警，仅以此作为总结。

## What?

配置系统是什么，可以简单理解为管理配置信息的系统，配置信息记录了服务器或者生产环境所需的一些信息（可以各种各样，完全可自定义）。

## Why?

.NET配置系统的优点在哪里？传统的web.config无法完成配置集群的管理，已经落后于时代的步伐，且微软官方已不再推荐此种用法。这些零零散散的方式管理起来很复杂也很繁琐，而配置系统可以跟踪全部配置的改变，并且可以按照优先级进行覆盖。

## How?

### 开发工具

- 开发系统：Win11 Enterprise Preview

- 笔者开发工具主要为三个：Jetbrains Rider(目前主力开发工具1)、VS Code(主力开发工具2)、Microsoft Visual Studio2022 Preview(曾经的主力开发工具)

### 前置条件

- 默认已掌握基础的.NET开发及DI（Dependency Injection)的概念和简单使用。

### 几种配置方式

1. 选项方式读取Json配置文件、其他配置提供者：包括命令行、环境变量等 

 （1）为项目安装Nuget包：

  - Microsoft.Extensions.DependencyInjection：依赖注入框架
  - Microsoft.Extensions.Configuration:配置框架
  - Microsoft.Extensions.Configuration.Json：配置框架Json扩展
  - Microsoft.Extensions.Options:选项扩展
  - Microsoft.Extensions.Configuration.Binder：选项绑定 
  - MailKit:用于项目演示用的邮箱开源框架（微软现推荐使用的邮箱框架，老的方式System.Net.Mail已不推荐）

 （2）编写Json配置文件，例如mail.config：

```json
{
"name": "albertzhao",
"MailBodyEntity": {
"MailTextBody": "Hi everyone,this mail is a test mail from albertzhao.",
"MailBodyType": "Plain",
"MailFileType": null,
"MailFileSubType": null,
"MailFilePath": null,
"Recipients": [ "2506747342@qq.com"],
"Cc": null,
"Sender": "AlbertZhao",
"SenderAddress": "szdxzhy@outlook.com",
"Subject": "TestMailKit",
"Body": "xxxx"
 },
"SendServerConfigurationEntity": {
"SmtpHost": "smtp-mail.outlook.com",
"SmtpPort": 587,
"IsSsl": true,
"MailEncoding": null}
}
```
（3）将Json文件转换成C#实体类：
```C#
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
```
(4）添加邮箱接口IMailService和实现类MailService以及依赖注入扩展类MailServiceExtensions(扩展Microsoft.Extensions.DependencyInjection)
- IMailService：定义了三个通用方法，邮件的发送、接收、下载
```C#
public interface IMailService
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <returns></returns>
        SendResultEntity SendMail();

        /// <summary>
        /// 接收邮件
        /// </summary>
        void ReceiveEmail();

        /// <summary>
        /// 下载邮件内容
        /// </summary>
        void DownloadBodyParts();
    }
```
- MailSevice：实现IMailService的接口，需要用选项方式在构造函数中注入,当ConfigurationBuilder.Build()并将扁平化字符串绑定到实体类后在调用DI的时候会将options转变为MailKitRoot类：
```C#
private readonly IOptionsSnapshot<MailKitRoot> options;
public MailService(IOptionsSnapshot<MailKitRoot> options)
{
    this.options = options;
}
```
- MailServiceExtension:方便使用，DI的扩展方法,此扩展方法将服务容器中绑定IMailService接口和实现类MailService，如果需要增加或扩展IMail接口的实现类，直接改变这里即可，无需动原业务代码任何地方。这就是DI的强大之处，强大的解耦能力，依赖接口而非具体实现类。
```C#
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MailServiceExtensions
    {
        public static void AddMailService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMailService, MailService>();
        }
    }
}
```
（5）在具体业务逻辑类中实现想要的业务，例如想调用SendMail()方法：这里首先进行DI容器的申请ServiceCollection，之后通过扩展方法，将接口和类注入进去，设置ConfigurationBuilder来通过选项方式配置，下面代码展示了命令行、环境变量、Json方式、数据库方式，以及如何通过AddOptions进行绑定到实体类的。最后的使用是容器Build一个服务提供者，调用服务提供者的获取服务方法SendMail()。关于AddOption()方法是如何将扁平化字符串转换到类的，不在本篇文章讨论请自行阅读源码（提示：一个返回的是IConfigurationRoot的，一个是IServiceCollection,其中有Dictionary来将字符串的键和值存储进去，然后进行反射解析type和property来CreateInstance，实现字符串到实体类的转换。
```C#
/// <summary>
/// <see cref="Test"/>
/// </summary>
/// <remarks>This main method is used for teaching DI.<remarks>
/// <param name="args"></param>
static void Main(string[] args)
{
    //依赖注入的容器Collection
    var serviceCollection = new ServiceCollection();
    //注入Config和Log
    serviceCollection.AddConfigService();
    serviceCollection.AddLogService();
    serviceCollection.AddMailService();
    //设置Option
    ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
    
    //[多配置源问题--后面的配置覆盖前面的配置]
    //通过数据库来获取中心配置,先写死进行测试
    //SqlConnection需要安装Nuget:System.Data.SqlClient
    //AddDbConfiguration来自Zack.AnyDBConfigProvider
    string strConfigFromSqlserver = "Server=192.168.0.238;Database=AlbertXDataBase;Trusted_Connection=True;";
    configurationBuilder.AddDbConfiguration(() => new SqlConnection(strConfigFromSqlserver),
        reloadOnChange: true,
        reloadInterval: TimeSpan.FromSeconds(2),
        tableName:"T_Configs");
    //如果要启用本地Json文件读取，则启用下面的代码，通过AddJsonFile来加载相关扁平化配置信息。
    configurationBuilder.AddJsonFile("AlbertConfig/mailconfig.json", false, true);
    //控制台
    //configurationBuilder.AddCommandLine(args);
    //环境变量
    configurationBuilder.AddEnvironmentVariables("Albert_");
    //从不泄密文件中读取账号密码：Secrets.Json
    configurationBuilder.AddUserSecrets<Program>();
    
    //从mailconfig读取回来的根节点
    var rootConfig = configurationBuilder.Build();
    //绑定根节点到MailKitRoot实体对象上
    //这边的GetSection里面的字段是Json字符串的字段，一定要注意这名字和实体类的属性名
    //一定要相同，不然无法绑定成功
    //ToDo:NetCore下的AddOption原理剖析。
    serviceCollection.AddOptions().Configure<MailKitRoot>(e => rootConfig.Bind(e))
        .Configure<MailBodyEntity>(e => rootConfig.GetSection("MailBodyEntity").Bind(e))
        .Configure<SendServerConfigurationEntity>(e => rootConfig.GetSection("SendServerConfigurationEntity").Bind(e));
    //使用DI,Build一个服务提供者
    using (var sp = serviceCollection.BuildServiceProvider())
    {
        var sendMailResult = sp.GetRequiredService<IMailService>().SendMail();
        Console.WriteLine(sendMailResult.ResultInformation);
        Console.WriteLine(sendMailResult.ResultStatus);
    }
}
```

### 开发属于自己的配置提供者

1. 开发Web.config配置提供者
   - 主要是开发ConfigurationProvider，需要开发两个类，一个直接或间接实现IConfigurationProvider(ConfigurationProvider\FileConfigurationProvider)，另一个实现IConfigurationSource(FileConfigurationSource)，在Build方法中返回ConfigurationProvider对象，添加Nuget包：Microsoft.Extensions.Configuration和Microsoft.Extensions.Configuration.FileExtensions。创建类FxConfigProvider，继承自FileConfigurationProvider抽象类，重写Load方法，其中Stream是文件流，从FileConfigurationSource中获取返回信息。在Load方法中将字符串拍平，通常的格式是“{第一级名称}：第二级名称"作为键，第二级名称作为值。同时在FxConfigurationProvider复写方法Load中，最后需要添加this.Data = data,将生成的字典赋值给它。
```C#
 class FxConfigProvider : FileConfigurationProvider {
    //进行一个转换,将src类型转换成基类FileConfigurationSource，这样才能够满足FileConfigurationProvider的构造函数
    public FxConfigProvider(FxConfigSource src):base(src) {
    }
    //此处stream是要读取的web.config的文件流
    public override void Load(Stream stream) {
        //此处是字典忽略大小写
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        XmlDocument xml = new XmlDocument();
        xml.Load(stream);
        var cdNodes = xml.SelectNodes("/configuration/connectionStrings/add");
        //Cast類型强轉
        foreach (XmlNode item in cdNodes.Cast<XmlNode>()) {
            string name = item.Attributes["name"].Value;
            string connectString = item.Attributes["connectString"].Value;
            //扁平化的根節點為connectStrings
            //[name1:{connectString:"xxx",providerName:"xxx"},name2:{connectString:"xxx",providerName:"xxx"}]
            //以{name}:connectString和{name}:providerName为键
            data[$"{name}:connectString"] = connectString;
            var providerName = item.Attributes["providerName"];
            if (providerName != null) {
                data[$"{name}:providerName"] = providerName.Value;
            }
        }
        var appSettingsNodes = xml.SelectNodes("/configuration/appSettings/add");
        foreach (XmlNode item in appSettingsNodes.Cast<XmlNode>()) {
            string key = item.Attributes["key"].Value;
            key = key.Replace(".", ":");
            string value = item.Attributes["value"].Value;
            data[key] = value;
        }
        this.Data = data;
    }
}

class FxConfigSource : FileConfigurationSource {
        public override IConfigurationProvider Build(IConfigurationBuilder builder) {
            //Called to use any default settings on the builder like the FileProvider or FileLoadExceptionHandler.
            EnsureDefaults((builder));//处理默认值等问题，
            return new FxConfigProvider(this);
        }
    }
```

2. 开发关系型数据库配置提供者
   方法和上述的类似，实现两个接口，但是其中的处理比较复杂，项目位于Github上，自行查阅和学习：https://github.com/yangzhongke/Zack.AnyDBConfigProvider.git

### 配置的优先级及配置信息隐私问题

1. 多配置源的优先级
   ConfigurationProvider内置支持覆盖，后面的配置覆盖前面的，同时数据库中的配置也可以进行覆盖，自增列覆盖前面的，前面不存在的配置后面存在亦可以。

2. 配置信息隐私问题
   安装Nuget:Microsoft.Extensions.Configuration.UserSecrets,在Visual Studio中右键项目，Manager UserSecrets，会在C盘或者AppDomain下的的目录中生成隐私Json配置文件，这个主要用于在实际开发中不想将隐私信息上传到源代码里面，本地删除或者重新装电脑则需要再单独配置，再实际生成环境中不适用，需要考虑加密问题，后续将出一个加密的模块单独去讨论。

### 相关源代码下载地址

- Gitee(国内)：https://gitee.com/hongyongzhao/dot-net_-config-system.git

- GitHub(国际):https://github.com/AlbertZhaohongyong/DotNet_ConfigSystem.git
