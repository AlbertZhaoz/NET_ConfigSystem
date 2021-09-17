using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetSystem_JsonProvider {
    //主要是提供参数使用
    class FxConfigSource : FileConfigurationSource {
        public override IConfigurationProvider Build(IConfigurationBuilder builder) {
            //Called to use any default settings on the builder like the FileProvider or FileLoadExceptionHandler.
            EnsureDefaults((builder));//处理默认值等问题，
            return new FxConfigProvider(this);
        }
    }
}
