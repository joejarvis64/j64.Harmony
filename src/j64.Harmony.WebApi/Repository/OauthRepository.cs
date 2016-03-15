using Newtonsoft.Json;
using System.IO;
using j64.Harmony.WebApi.Models;

namespace j64.Harmony.WebApi.Repository
{
    public class OauthRepository
    {
        public static string RepositoryFile { get; set; } = "SmartThings.json";

        public static OauthInfo Get()
        {
            OauthInfo oai = null;

            if (File.Exists(RepositoryFile) == false)
                return new OauthInfo();

            // Read the settings now
            using (StreamReader file = System.IO.File.OpenText(RepositoryFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                oai = (OauthInfo)serializer.Deserialize(file, typeof(OauthInfo));
            }

            return oai;
        }

        public static OauthInfo Save(OauthInfo oai)
        {
            using (StreamWriter file = System.IO.File.CreateText(RepositoryFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, oai);
            }

            return oai;
        }
    }
}
