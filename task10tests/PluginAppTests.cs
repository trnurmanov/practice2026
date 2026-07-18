using CommandLib;
using PluginApp;
using System.Reflection;
using Xunit;

namespace PluginAppTests
{
    public class PluginLoaderTests
    {

        [Fact]
        public void LoadPlugins_ShouldReturnEmpty_WhenNoDlls()
        {

            var pluginLoader = new PluginLoader();
            string tempDirectory = Path.Combine(Path.GetTempPath(), "empty_plugins_" + Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempDirectory);

                var loadedPlugins = pluginLoader.LoadPlugins(tempDirectory);

                Assert.NotNull(loadedPlugins);
                Assert.Equal(0, loadedPlugins.Count);
            }
            finally
            {

                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

    }
}

