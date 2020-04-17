using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
namespace Suncor.LessonsLearnedMP.Business
{

    //StringBuilder sb = new StringBuilder();

    //var connection = new ActiveDirectoryConnection();
    //var service = new ExchangeAddressListService(connection);
    //foreach (var addressList in service.GetGlobalAddressLists())
    //{
    //    sb.AppendLine(string.Format("addressList.Name = {0}", addressList.Name));
    //    foreach (var searchResult in addressList.GetMembers())
    //    {
    //        sb.AppendLine(string.Format("\t{0}", searchResult.Properties["name"][0]));
    //    }
    //}

    //foreach (var addressList in service.GetAllAddressLists())
    //{
    //    sb.AppendLine(string.Format("addressList.Name = {0}", addressList.Name));
    //    foreach (var searchResult in addressList.GetMembers())
    //    {
    //        sb.AppendLine(string.Format("\t{0}", searchResult.Properties["name"][0]));
    //    }
    //}

    //foreach (var addressList in service.GetSystemAddressLists())
    //{
    //    sb.AppendLine(string.Format("addressList.Name = {0}", addressList.Name));
    //    foreach (var searchResult in addressList.GetMembers())
    //    {
    //        sb.AppendLine(string.Format("\t{0}", searchResult.Properties["name"][0]));
    //    }
    //}

    //string result = sb.ToString();

    public class ActiveDirectoryConnection
    {
        public DirectoryEntry GetLdapDirectoryEntry(string path)
        {
            return GetDirectoryEntry(path, "LDAP");
        }

        public DirectoryEntry GetGCDirectoryEntry(string path)
        {
            return GetDirectoryEntry(path, "GC");
        }

        private DirectoryEntry GetDirectoryEntry(string path, string protocol)
        {
            var ldapPath = string.IsNullOrEmpty(path) ? string.Format("{0}:", protocol) : string.Format("{0}://{1}", protocol, path);
            return new DirectoryEntry(ldapPath);
        }
    }

    public class ExchangeAddressListService
    {
        private readonly ActiveDirectoryConnection _Connection;

        public ExchangeAddressListService(ActiveDirectoryConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            _Connection = connection;
        }

        public IEnumerable<AddressList> GetGlobalAddressLists()
        {
            return GetAddressLists("CN=All Global Address Lists");
        }

        public IEnumerable<AddressList> GetAllAddressLists()
        {
            return GetAddressLists("CN=All Address Lists");
        }
        public IEnumerable<AddressList> GetSystemAddressLists()
        {
            return GetAddressLists("CN=All System Address Lists");
        }

        private IEnumerable<AddressList> GetAddressLists(string containerName)
        {
            string exchangeRootPath;
            using (var root = _Connection.GetLdapDirectoryEntry("RootDSE"))
            {
                exchangeRootPath = string.Format("CN=Microsoft Exchange, CN=Services, {0}", root.Properties["configurationNamingContext"].Value);
            }
            string companyRoot;
            using (var exchangeRoot = _Connection.GetLdapDirectoryEntry(exchangeRootPath))
            using (var searcher = new DirectorySearcher(exchangeRoot, "(objectclass=msExchOrganizationContainer)"))
            {
                companyRoot = (string)searcher.FindOne().Properties["distinguishedName"][0];
            }

            var globalAddressListPath = string.Format(containerName + ",CN=Address Lists Container, {0}", companyRoot);
            var addressListContainer = _Connection.GetLdapDirectoryEntry(globalAddressListPath);

            using (var searcher = new DirectorySearcher(addressListContainer, "(objectClass=addressBookContainer)"))
            {
                searcher.SearchScope = SearchScope.OneLevel;
                using (var searchResultCollection = searcher.FindAll())
                {
                    foreach (SearchResult addressBook in searchResultCollection)
                    {
                        yield return
                            new AddressList((string)addressBook.Properties["distinguishedName"][0], _Connection);
                    }
                }
            }
        }
    }

    public class AddressList
    {
        private readonly ActiveDirectoryConnection _Connection;
        private readonly string _Path;

        private DirectoryEntry _DirectoryEntry;

        internal AddressList(string path, ActiveDirectoryConnection connection)
        {
            _Path = path;
            _Connection = connection;
        }

        private DirectoryEntry DirectoryEntry
        {
            get
            {
                if (_DirectoryEntry == null)
                {
                    _DirectoryEntry = _Connection.GetLdapDirectoryEntry(_Path);
                }
                return _DirectoryEntry;
            }
        }

        public string Name
        {
            get { return (string)DirectoryEntry.Properties["name"].Value; }
        }

        public IEnumerable<SearchResult> GetMembers(params string[] propertiesToLoad)
        {
            var rootDse = _Connection.GetGCDirectoryEntry(string.Empty);
            var searchRoot = rootDse.Children.Cast<DirectoryEntry>().First();
            using (var searcher = new DirectorySearcher(searchRoot, string.Format("(showInAddressBook={0})", _Path)))
            {
                if (propertiesToLoad != null)
                {
                    searcher.PropertiesToLoad.AddRange(propertiesToLoad);
                }
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PageSize = 512;
                do
                {
                    using (var result = searcher.FindAll())
                    {
                        foreach (SearchResult searchResult in result)
                        {
                            yield return searchResult;
                        }
                        if (result.Count < 512) break;
                    }
                } while (true);
            }
        }
    }
}
