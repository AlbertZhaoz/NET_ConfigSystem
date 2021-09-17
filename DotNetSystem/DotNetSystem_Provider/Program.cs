using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetSystem_JsonProvider
{
    class Program
    {
        /// <summary>
        /// </summary>
        /// <remarks>This main method is used for teaching DI.</remarks>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //依赖注入的容器Collection
            var serviceCollection = new ServiceCollection();
            //注入Config和Log
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
    }
}