/// -----------------------------------------------------------------------------------------------------------
/// Module      :  WCFMockAdapter.cs
/// Description :  The main adapter class which inherits from Adapter
/// -----------------------------------------------------------------------------------------------------------

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Description;

using Microsoft.ServiceModel.Channels.Common;
#endregion

namespace TransMock.Wcf.Adapter
{
    public class MockAdapter : Microsoft.ServiceModel.Channels.Common.Adapter
    {
        // Scheme associated with the adapter
        internal const string SCHEME = "mock";
        // Namespace for the proxy that will be generated from the adapter schema
        internal const string SERVICENAMESPACE = "http://www.transmock.com/Adapter/BizTalk";
        // Initializes the AdapterEnvironmentSettings class
        private static AdapterEnvironmentSettings environmentSettings = new AdapterEnvironmentSettings();

        #region Custom Generated Fields

        //private string uRI;


        private string encoding;


        private string promotedProperties;

        #endregion Custom Generated Fields

        #region  Constructor

        /// <summary>
        /// Initializes a new instance of the WCFMockAdapter class
        /// </summary>
        public MockAdapter()
            : base(environmentSettings)
        {
            Settings.Metadata.DefaultMetadataNamespace = SERVICENAMESPACE;
        }

        /// <summary>
        /// Initializes a new instance of the WCFMockAdapter class with a binding
        /// </summary>
        public MockAdapter(MockAdapter binding)
            : base(binding)
        {
            //this.URI = binding.URI;
            this.Encoding = binding.Encoding;
            this.PromotedProperties = binding.PromotedProperties;
        }

        #endregion Constructor

        #region Custom Generated Properties

        //[System.Configuration.ConfigurationProperty("uRI", DefaultValue = "mock://")]
        //public string URI
        //{
        //    get
        //    {
        //        return this.uRI;
        //    }
        //    set
        //    {
        //        this.uRI = value;
        //    }
        //}



        [System.Configuration.ConfigurationProperty("Encoding")]
        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }



        [System.Configuration.ConfigurationProperty("PromotedProperties")]
        public string PromotedProperties
        {
            get
            {
                return this.promotedProperties;
            }
            set
            {
                this.promotedProperties = value;
            }
        }

        #endregion Custom Generated Properties

        #region Public Properties

        /// <summary>
        /// Gets the URI transport scheme that is used by the adapter
        /// </summary>
        public override string Scheme
        {
            get
            {
                return SCHEME;
            }
        }

        #endregion Public Properties

        #region Protected Methods

        /// <summary>
        /// Creates a ConnectionUri instance from the provided Uri
        /// </summary>
        protected override ConnectionUri BuildConnectionUri(Uri uri)
        {
            return new MockAdapterConnectionUri(uri);
        }

        /// <summary>
        /// Builds a connection factory from the ConnectionUri and ClientCredentials
        /// </summary>
        protected override IConnectionFactory BuildConnectionFactory(
            ConnectionUri connectionUri
            , ClientCredentials clientCredentials
            , System.ServiceModel.Channels.BindingContext context)
        {
            return new MockAdapterConnectionFactory(connectionUri, clientCredentials, this);
        }

        /// <summary>
        /// Returns a clone of the adapter object
        /// </summary>
        protected override Microsoft.ServiceModel.Channels.Common.Adapter CloneAdapter()
        {
            return new MockAdapter(this);
        }

        /// <summary>
        /// Indicates whether the provided TConnectionHandler is supported by the adapter or not
        /// </summary>
        protected override bool IsHandlerSupported<TConnectionHandler>()
        {
            return (
                  //typeof(IAsyncOutboundHandler) == typeof(TConnectionHandler)
                typeof(IOutboundHandler) == typeof(TConnectionHandler)
                //|| typeof(IAsyncInboundHandler) == typeof(TConnectionHandler)
                || typeof(IInboundHandler) == typeof(TConnectionHandler));
        }

        /// <summary>
        /// Gets the namespace that is used when generating schema and WSDL
        /// </summary>
        protected override string Namespace
        {
            get
            {
                return SERVICENAMESPACE;
            }
        }

        #endregion Protected Methods
    }
}