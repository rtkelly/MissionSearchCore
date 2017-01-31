using MissionSearch.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MissionSearch.Clients.Solr
{
    public class SolrResources<T> where T : ISearchDocument 
    {
        SolrClient<T> Client { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public SolrResources(SolrClient<T> client)
        {
            Client = client;
        }

        /// <summary>
        /// 
        /// </summary>
        public SolrResources()
        {
            Client = SearchFactory<T>.SearchClient as SolrClient<T>;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ManagedResourcesContainer GetManagedResources()
        {
            var endPointGetResources = string.Format("{0}/schema/managed", Client.SrchConnStr);

            var json = HttpClient.GetRequest(endPointGetResources);
          
           var resourceContainer = JsonConvert.DeserializeObject<ManagedResourcesContainer>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                
            });


           return resourceContainer;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ManagedResource> GetManagedSynonymResources()
        {
            //var resources = new List<ManagedResource>();

            var endPointGetResources = string.Format("{0}/schema/managed", Client.SrchConnStr);

            var json = HttpClient.GetRequest(endPointGetResources);

            var resourceContainer = JsonConvert.DeserializeObject<ManagedResourcesContainer>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,

            });

            var resources = resourceContainer.managedResources.Where(r => r.resourceId.StartsWith("/schema/analysis/synonyms")).ToList();

            return resources;

        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ManagedResourcesContainer GetManagedResource()
        {
            var endPointGetResources = string.Format("{0}/schema/managed", Client.SrchConnStr);

            var json = HttpClient.GetRequest(endPointGetResources);
           
            var resourceContainer = JsonConvert.DeserializeObject<ManagedResourcesContainer>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,

            });


            return resourceContainer;

        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public List<Synonym> GetSynonyms(string language)
        {
            var synonyms = new List<Synonym>();

            var EndPointGetResources = string.Format("{0}/schema/analysis/synonyms/{1}", Client.SrchConnStr, language);

            var json = HttpClient.GetRequest(EndPointGetResources);
            
            var synounmContainer = JsonConvert.DeserializeObject<SynonymMappingsContainer>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });

            foreach(var synonymMap in synounmContainer.synonymMappings.managedMap)
            {
                  synonyms.Add(new Synonym()
                    {
                        Term = synonymMap.Key,
                        Synonyms = string.Join(",", synonymMap.Value),
                        
                    });

             }

            return synonyms;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public List<Synonym> GetDistinctSynonyms(string language)
        {
            var synonyms = new List<Synonym>();

            var endPointGetResources = string.Format("{0}/schema/analysis/synonyms/{1}", Client.SrchConnStr, language);

            var json = HttpClient.GetRequest(endPointGetResources);

            var synounmContainer = JsonConvert.DeserializeObject<SynonymMappingsContainer>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });
            
            foreach(var synonymMap in synounmContainer.synonymMappings.managedMap)
            {
                var found = false;

                foreach (var m in synonyms)
                {
                    var mList = m.SynonymList;

                    if(mList.SequenceEqual(synonymMap.Value))
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    synonyms.Add(new Synonym()
                    {
                        Term = synonymMap.Key,
                        Synonyms = string.Join(",", synonymMap.Value),
                        
                    });
                }
                //else
                //{
                   // var map = synonyms.FirstOrDefault(m => m.Term == synonymMap.Key);

                    //if (map != null)
                      //  map.Bidirectional = true;
                //}
            }

            return synonyms;
        }

        public Synonym GetSynonym(string language, string term)
        {
            var endPointGetResources = string.Format("{0}/schema/analysis/synonyms/{1}/{2}", Client.SrchConnStr, language, term);

            var json = HttpClient.GetRequest(endPointGetResources);
           
            var synonymObj = JsonConvert.DeserializeObject<dynamic>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });

            var synonyms = synonymObj[term];

            var m = new Synonym()
            {
                Term = term,
                Synonyms = string.Join("," , synonyms),
            };

            return m;

        }

        public void DeleteSynonym(string language, string name)
        {
            try
            {
                var endPointGetResources = string.Format("{0}/schema/analysis/synonyms/{1}/{2}", Client.SrchConnStr, language, name);

                HttpClient.CallWebRequest(endPointGetResources, "DELETE");
            }
            catch (WebException ex)
            {
                HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                
                // throw exception unless it's a 404
                if (errorResponse != null && errorResponse.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
            
        }

        public void ReloadCore()
        {
            Client.Reload();
        }

        public void AddSynonym(string language, Synonym synonym)
        {
            var trimmedMap = synonym.SynonymList.Select(str => str.ToLower().Trim()).Where(str => !string.IsNullOrEmpty(str)).ToList();
                        
            var settings = new JsonSerializerSettings();
            
            //var jsonDoc = JsonConvert.SerializeObject(trimmedMap, settings);
            /*
            if (synonym.Bidirectional)
            {
                jsonDoc = JsonConvert.SerializeObject(trimmedMap, settings);
                
            }
            else
            {
                var dict = new Dictionary<string, List<string>>();
                dict.Add(synonym.Term.ToLower(), trimmedMap);
                jsonDoc = JsonConvert.SerializeObject(dict, settings);
            }
             */

            var dict = new Dictionary<string, List<string>>();
            dict.Add(synonym.Term.ToLower(), trimmedMap);
            var jsonDoc = JsonConvert.SerializeObject(dict, settings);

            var endPointGetResources = string.Format("{0}/schema/analysis/synonyms/{1}", Client.SrchConnStr, language);

            HttpClient.PostJson(endPointGetResources, jsonDoc);
        }
        

    }
       
    public class ManagedResourcesContainer
    {
        public List<ManagedResource> managedResources { get; set; }

    }

    public class ManagedResource
    {
        public string resourceId { get; set; }
        public string theclass { get; set; }
        public int numObservers { get; set; }
    }

    public class SynonymMappingsContainer
    {
        public SynonymMappings synonymMappings { get; set; }

    }

    public class SynonymMappings
    {
        public Dictionary<string, List<string>> managedMap { get; set; }
                
    }
    
    public class Synonym
    {
        public string Term { get; set; }
        public string Synonyms { get; set; }
        //public bool Bidirectional { get; set; }

        public List<string> SynonymList { get { return Synonyms.Split(',').ToList(); } }
     
    }
    
    
}
/*
     
 * {
"responseHeader":{
"status":0,
"QTime":4},
"cat":["feline",
"kitten",
"kitty"]}

      
     
     
 {
"responseHeader":{
"status":0,
"QTime":2},
"synonymMappings":{
"initArgs":{"ignoreCase":false},
"initializedOn":"2016-05-16T17:11:10.572Z",
"updatedSinceInit":"2016-05-16T18:32:38.279Z",
"managedMap":{
  "dog":["Kanine",
    "Mutt",
    "hound",
    "kanine",
    "mutt",
    "puppy"],
  "mad":["angry",
    "upset"]}}} 
     
      
     
     
 * 
 * {
"responseHeader":{
"status":0,
"QTime":3
},
"managedResources":[
{
  "resourceId":"/schema/analysis/stopwords/english",
  "class":"org.apache.solr.rest.schema.analysis.ManagedWordSetResource",
  "numObservers":"1"
},
{
  "resourceId":"/schema/analysis/synonyms/english",
  "class":"org.apache.solr.rest.schema.analysis.ManagedSynonymFilterFactory$SynonymManager",
  "numObservers":"1"
}
]
}
 * */

