﻿using System;
using System.Collections.Immutable;
using System.ComponentModel;
using TSSArt.StateMachine.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	public class StateMachineHostBuilder
	{
		private ImmutableDictionary<string, string>.Builder?      _configuration;
		private ImmutableArray<ICustomActionFactory>.Builder?     _customActionFactories;
		private ImmutableArray<IDataModelHandlerFactory>.Builder? _dataModelHandlerFactories;
		private ImmutableArray<IIoProcessorFactory>.Builder?      _ioProcessorFactories;
		private ILogger?                                          _logger;
		private PersistenceLevel                                  _persistenceLevel;
		private ImmutableArray<IResourceLoader>.Builder?          _resourceLoaders;
		private ImmutableArray<IServiceFactory>.Builder?          _serviceFactories;
		private IStorageProvider?                                 _storageProvider;
		private TimeSpan                                          _suspendIdlePeriod;
		private Uri?                                              _baseUri;
		private bool                                              _verboseValidation = true;

		public StateMachineHost Build()
		{
			var option = new StateMachineHostOptions
						 {
								 IoProcessorFactories = _ioProcessorFactories?.ToImmutable() ?? default,
								 ServiceFactories = _serviceFactories?.ToImmutable() ?? default,
								 DataModelHandlerFactories = _dataModelHandlerFactories?.ToImmutable() ?? default,
								 CustomActionFactories = _customActionFactories?.ToImmutable() ?? default,
								 ResourceLoaders = _resourceLoaders?.ToImmutable() ?? default,
								 Configuration = _configuration?.ToImmutable(),
								 BaseUri = _baseUri,
								 Logger = _logger,
								 PersistenceLevel = _persistenceLevel,
								 StorageProvider = _storageProvider,
								 SuspendIdlePeriod = _suspendIdlePeriod,
								 VerboseValidation = _verboseValidation
						 };

			return new StateMachineHost(option);
		}

		public StateMachineHostBuilder SetLogger(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			return this;
		}

		public StateMachineHostBuilder DisableVerboseValidation()
		{
			_verboseValidation = false;

			return this;
		}

		public StateMachineHostBuilder SetSuspendIdlePeriod(TimeSpan suspendIdlePeriod)
		{
			if (suspendIdlePeriod <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(suspendIdlePeriod));

			_suspendIdlePeriod = suspendIdlePeriod;

			return this;
		}

		public StateMachineHostBuilder AddResourceLoader(IResourceLoader resourceLoader)
		{
			if (resourceLoader == null) throw new ArgumentNullException(nameof(resourceLoader));

			(_resourceLoaders ??= ImmutableArray.CreateBuilder<IResourceLoader>()).Add(resourceLoader);

			return this;
		}

		public StateMachineHostBuilder SetPersistence(PersistenceLevel persistenceLevel, IStorageProvider storageProvider)
		{
			if (!Enum.IsDefined(typeof(PersistenceLevel), persistenceLevel)) throw new InvalidEnumArgumentException(nameof(persistenceLevel), (int) persistenceLevel, typeof(PersistenceLevel));

			_persistenceLevel = persistenceLevel;
			_storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));

			return this;
		}

		public StateMachineHostBuilder AddIoProcessorFactory(IIoProcessorFactory ioProcessorFactory)
		{
			if (ioProcessorFactory == null) throw new ArgumentNullException(nameof(ioProcessorFactory));

			(_ioProcessorFactories ??= ImmutableArray.CreateBuilder<IIoProcessorFactory>()).Add(ioProcessorFactory);

			return this;
		}

		public StateMachineHostBuilder AddServiceFactory(IServiceFactory serviceFactory)
		{
			if (serviceFactory == null) throw new ArgumentNullException(nameof(serviceFactory));

			(_serviceFactories ??= ImmutableArray.CreateBuilder<IServiceFactory>()).Add(serviceFactory);

			return this;
		}

		public StateMachineHostBuilder AddDataModelHandlerFactory(IDataModelHandlerFactory dataModelHandlerFactory)
		{
			if (dataModelHandlerFactory == null) throw new ArgumentNullException(nameof(dataModelHandlerFactory));

			(_dataModelHandlerFactories ??= ImmutableArray.CreateBuilder<IDataModelHandlerFactory>()).Add(dataModelHandlerFactory);

			return this;
		}

		public StateMachineHostBuilder AddCustomActionFactory(ICustomActionFactory customActionFactory)
		{
			if (customActionFactory == null) throw new ArgumentNullException(nameof(customActionFactory));

			(_customActionFactories ??= ImmutableArray.CreateBuilder<ICustomActionFactory>()).Add(customActionFactory);

			return this;
		}

		public StateMachineHostBuilder SetConfigurationValue(string key, string value)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			(_configuration ??= ImmutableDictionary.CreateBuilder<string, string>())[key] = value ?? throw new ArgumentNullException(nameof(value));

			return this;
		}

		public StateMachineHostBuilder SetBaseUri(Uri uri)
		{
			_baseUri = uri ?? throw new ArgumentNullException(nameof(uri));

			return this;
		}
	}
}