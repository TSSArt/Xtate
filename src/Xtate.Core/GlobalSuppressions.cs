﻿#region Copyright © 2019-2020 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

// @formatter:off

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(category:"Design", checkId:"CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Service.SimpleServiceBase")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineController")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineHostContext")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.FinalFluentBuilder`1")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.IFinalBuilder")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.IParallelBuilder")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.IStateBuilder")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.ParallelFluentBuilder`1")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.StateFluentBuilder`1")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1030:Use events where appropriate", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Builder.TransitionFluentBuilder`1")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.CustomActionDispatcher.SetupExecutor(System.Collections.Immutable.ImmutableArray{Xtate.CustomAction.ICustomActionFactory},System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask")][assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IoProcessor.NamedIoProcessor.StartListener~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.FileResourceLoader.RequestXmlReader(System.Uri,System.Xml.XmlReaderSettings,System.Xml.XmlParserContext,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{System.Xml.XmlReader}")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IoProcessor.NamedIoProcessorFactory.GetHostName~System.String")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Persistence.StreamStorage.DisposeAsync~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.ResxResourceLoader.RequestXmlReader(System.Uri,System.Xml.XmlReaderSettings,System.Xml.XmlParserContext,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{System.Xml.XmlReader}")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.XmlDirector`1.Populate``1(``0,Xtate.Scxml.XmlDirector`1.Policy{``0})~``0")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.XmlDirector`1.PopulateAttributes``1(``0,Xtate.Scxml.XmlDirector`1.Policy{``0},Xtate.Scxml.XmlDirector`1.Policy{``0}.ValidationContext)")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.XmlDirector`1.PopulateElements``1(``0,Xtate.Scxml.XmlDirector`1.Policy{``0},Xtate.Scxml.XmlDirector`1.Policy{``0}.ValidationContext)")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Service.SimpleServiceBase.DisposeAsync~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Service.SimpleServiceBase.Start(System.Uri,Xtate.InvokeData,Xtate.Service.IServiceCommunication)")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineController.DelayedFire(Xtate.StateMachineController.ScheduledEvent,System.Int32)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineHost.CompleteAsync(Xtate.StateMachineHostContext,Xtate.Service.IService,Xtate.StateMachineController,Xtate.SessionId,Xtate.InvokeId)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineHostContext.WaitAllAsync(System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineInterpreter.Error(System.Object,System.Exception,System.Boolean)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.WebResourceLoader.RequestXmlReader(System.Uri,System.Xml.XmlReaderSettings,System.Xml.XmlParserContext,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{System.Xml.XmlReader}")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1032:Implement standard exception constructors", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineValidationException")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.LazyId.Xtate#IObject#ToObject~System.Object")][assembly: SuppressMessage(category:"Globalization", checkId:"CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IdGenerator.NewGuidWithHash(System.Int32)~System.String")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "member", Target = "~P:Xtate.DataModel.DefaultDoneDataEvaluator.Xtate#IAncestorProvider#Ancestor")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "member", Target = "~P:Xtate.DataModel.DefaultInlineContentEvaluator.Xtate#IAncestorProvider#Ancestor")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DataModelHandlerBase")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultAssignEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultCancelEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultContentBodyEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultCustomActionEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultForEachEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultIfEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultInvokeEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultLogEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultRaiseEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultScriptEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.DefaultSendEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IoProcessor.IoProcessorBase")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1033:Interface methods should be callable by child types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Service.SimpleServiceBase")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1034:Nested types should not be visible", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Scxml.XmlDirector`1.Policy`1.ValidationContext")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1040:Avoid empty interfaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModel.IValueEvaluator")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1040:Avoid empty interfaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IElse")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1040:Avoid empty interfaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IEntity")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1040:Avoid empty interfaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IExecutableEntity")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1040:Avoid empty interfaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IIdentifier")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1040:Avoid empty interfaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IStateEntity")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.ScxmlSerializer.Build(Xtate.IStateMachine@,Xtate.StateMachineEntity@)")]
[assembly: SuppressMessage(category:"Design", checkId:"CA1062:Validate arguments of public methods", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.ErrorProcessorExtensions.AddError(Xtate.IErrorProcessor,System.Type,System.Object,System.String,System.Exception)")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IdGenerator.NewInvokeId(System.String,System.Int32)~System.String")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelList.Entry.GetHashCode~System.Int32")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelList.KeyValue.GetHashCode~System.Int32")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelValue.TryFromAnonymousType(System.Object,System.Collections.Generic.Dictionary{System.Object,System.Object}@,Xtate.DataModelValue@)~System.Boolean")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.FullUriComparer.GetHashCode(System.Uri)~System.Int32")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.InvokeId.InvokeUniqueIdEqualityComparer.GetHashCode(Xtate.InvokeId)~System.Int32")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IoProcessor.NamedIoProcessor.ExtractSessionId(System.Uri)~Xtate.SessionId")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.LazyId.GetHashCode~System.Int32")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.PrefixNamespace.GetHashCode~System.Int32")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.ScxmlDirector.AsEventDescriptorList(System.String)~System.Collections.Immutable.ImmutableArray{Xtate.IEventDescriptor}")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.ScxmlDirector.AsIdentifierList(System.String)~System.Collections.Immutable.ImmutableArray{Xtate.IIdentifier}")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.ScxmlDirector.AsLocationExpressionList(System.String)~System.Collections.Immutable.ImmutableArray{Xtate.ILocationExpression}")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.SessionId.#ctor(System.String)")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1307:Specify StringComparison", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineHostContext.GetService(Xtate.SessionId,System.Uri)~Xtate.Service.IService")]
[assembly: SuppressMessage(category:"Globalization", checkId:"CA1308:Normalize strings to uppercase", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Scxml.ScxmlDirector.AsEnum``1(System.String)~``0")]
[assembly: SuppressMessage(category:"Microsoft.Naming", checkId:"CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.Scxml.XmlDirector`1.Policy`1")]
[assembly: SuppressMessage(category:"Naming", checkId:"CA1710:Identifiers should have correct suffix", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModelArray")]
[assembly: SuppressMessage(category:"Naming", checkId:"CA1710:Identifiers should have correct suffix", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModelObject")]
[assembly: SuppressMessage(category:"Naming", checkId:"CA1720:Identifier contains type name", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModelValueType")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.AssignEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.CancelEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ConditionExpression")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ConfiguredStreamAwaitable")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ContentBody")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ContentEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.CustomActionEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DataModelEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.DoneDataEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ElseEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ElseIfEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.EventEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ExternalDataExpression")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ExternalScriptExpression")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.FinalEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.FinalizeEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ForEachEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.HistoryEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.IfEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.InitialEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.InlineContent")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.InterpreterOptions")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.InvokeData")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.InvokeEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.LocationExpression")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.LogEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.OnEntryEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.OnExitEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ParallelEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ParamEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.RaiseEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ScriptEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ScriptExpression")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.SendEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineOptions")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineOrigin")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.StateMachineVisitor.TrackList`1")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.TransitionEntity")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1815:Override equals and operator equals on value types", Justification = "<Pending>", Scope = "type", Target = "~T:Xtate.ValueExpression")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IoProcessor.NamedIoProcessor.ReceiveMessage(System.IO.Pipes.PipeStream,System.IO.MemoryStream,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Persistence.StreamStorage.CheckPoint(System.Int32,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Persistence.StreamStorage.ReadStream(System.IO.Stream,System.Int32,System.Boolean,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{Xtate.Persistence.InMemoryStorage}")]
[assembly: SuppressMessage(category:"Performance", checkId:"CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.ResxResourceLoader.Request(System.Uri,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{Xtate.Resource}")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2000:Dispose objects before losing scope", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.FileResourceLoader.RequestXmlReader(System.Uri,System.Xml.XmlReaderSettings,System.Xml.XmlParserContext,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{System.Xml.XmlReader}")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2000:Dispose objects before losing scope", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.IoProcessor.NamedIoProcessor.StartListener~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2000:Dispose objects before losing scope", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.ResxResourceLoader.RequestXmlReader(System.Uri,System.Xml.XmlReaderSettings,System.Xml.XmlParserContext,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{System.Xml.XmlReader}")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2000:Dispose objects before losing scope", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineHost.Xtate#IStateMachineHost#IsInvokeActive(Xtate.SessionId,Xtate.InvokeId)~System.Boolean")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2000:Dispose objects before losing scope", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.StateMachineHost.Xtate#IStateMachineHost#StartInvoke(Xtate.SessionId,Xtate.InvokeData,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2000:Dispose objects before losing scope", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.WebResourceLoader.RequestXmlReader(System.Uri,System.Xml.XmlReaderSettings,System.Xml.XmlParserContext,System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask{System.Xml.XmlReader}")]
[assembly: SuppressMessage(category:"Reliability", checkId:"CA2016:Forward the 'CancellationToken' parameter to methods that take one", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Service.SimpleServiceBase.Xtate#Service#IService#Destroy(System.Threading.CancellationToken)~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage(category:"Style",  checkId:"IDE0057:Use range operator", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.EventName.SetParts(System.Span{Xtate.IIdentifier},System.String)")]
[assembly: SuppressMessage(category:"Style", checkId:"IDE0016:Use 'throw' expression", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelList.CanSet(System.String,System.Boolean)~System.Boolean")]
[assembly: SuppressMessage(category:"Style", checkId:"IDE0016:Use 'throw' expression", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelList.Set(System.String,System.Boolean,Xtate.DataModelValue@,Xtate.DataModelList)")]
[assembly: SuppressMessage(category:"Style", checkId:"IDE0016:Use 'throw' expression", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelList.SetInternal(System.String,System.Boolean,Xtate.DataModelValue@,Xtate.DataModelAccess,Xtate.DataModelList,System.Boolean)~System.Boolean")]
[assembly: SuppressMessage(category:"Style", checkId:"IDE0057:Use range operator", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModel.None.NoneDataModelHandler.Build(Xtate.IConditionExpression@,Xtate.ConditionExpression@)")]
[assembly: SuppressMessage(category:"Style", checkId:"IDE0057:Use range operator", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Persistence.Bucket.StringKeyConverter`1.Write(System.String,System.Span{System.Byte})")]
[assembly: SuppressMessage(category:"Style", checkId:"IDE0066:Convert switch statement to expression", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.Persistence.Encode.Decode(System.ReadOnlySpan{System.Byte})~System.Int32")]
[assembly: SuppressMessage(category:"Usage", checkId:"CA2213:Disposable fields should be disposed", Justification = "<Pending>", Scope = "member", Target = "~F:Xtate.StateMachineHost._context")]
[assembly: SuppressMessage(category:"Usage", checkId:"CA2225:Operator overloads have named alternates", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelDateTime.op_Explicit(Xtate.DataModelDateTime)~System.DateTime")]
[assembly: SuppressMessage(category:"Usage", checkId:"CA2225:Operator overloads have named alternates", Justification = "<Pending>", Scope = "member", Target = "~M:Xtate.DataModelDateTime.op_Explicit(Xtate.DataModelDateTime)~System.DateTimeOffset")]
[assembly: SuppressMessage(category:"Usage", checkId:"CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:Xtate.InterpreterOptions.Configuration")]
[assembly: SuppressMessage(category:"Usage", checkId:"CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:Xtate.InterpreterOptions.Host")]