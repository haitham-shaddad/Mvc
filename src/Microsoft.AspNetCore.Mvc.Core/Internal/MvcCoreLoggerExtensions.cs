// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    internal static class MvcCoreLoggerExtensions
    {
        public const string ActionFilter = "Action Filter";
        private static readonly string[] _noFilters = new[] { "None" };

        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private static readonly Action<ILogger, string, Exception> _actionExecuting;
        private static readonly Action<ILogger, string, double, Exception> _actionExecuted;

        private static readonly Action<ILogger, string[], Exception> _challengeResultExecuting;

        private static readonly Action<ILogger, string, Exception> _contentResultExecuting;

        private static readonly Action<ILogger, string, string[], ModelValidationState, Exception> _actionMethodExecuting;
        private static readonly Action<ILogger, string, string, Exception> _actionMethodExecuted;

        private static readonly Action<ILogger, string, string[], Exception> _logFilterExecutionPlan;
        private static readonly Action<ILogger, string, string, Type, Exception> _beforeExecutingMethodOnFilter;
        private static readonly Action<ILogger, string, string, Type, Exception> _afterExecutingMethodOnFilter;
        private static readonly Action<ILogger, Type, Exception> _beforeExecutingActionResult;
        private static readonly Action<ILogger, Type, Exception> _afterExecutingActionResult;

        private static readonly Action<ILogger, string, Exception> _ambiguousActions;
        private static readonly Action<ILogger, string, string, IActionConstraint, Exception> _constraintMismatch;

        private static readonly Action<ILogger, string, Exception> _fileResultExecuting;

        private static readonly Action<ILogger, object, Exception> _authorizationFailure;
        private static readonly Action<ILogger, object, Exception> _resourceFilterShortCircuit;
        private static readonly Action<ILogger, object, Exception> _resultFilterShortCircuit;
        private static readonly Action<ILogger, object, Exception> _actionFilterShortCircuit;
        private static readonly Action<ILogger, object, Exception> _exceptionFilterShortCircuit;

        private static readonly Action<ILogger, string[], Exception> _forbidResultExecuting;
        private static readonly Action<ILogger, string, ClaimsPrincipal, Exception> _signInResultExecuting;
        private static readonly Action<ILogger, string[], Exception> _signOutResultExecuting;

        private static readonly Action<ILogger, int, Exception> _httpStatusCodeResultExecuting;

        private static readonly Action<ILogger, string, Exception> _localRedirectResultExecuting;

        private static readonly Action<ILogger, string, Exception> _objectResultExecuting;
        private static readonly Action<ILogger, string, Exception> _noFormatter;
        private static readonly Action<ILogger, IOutputFormatter, string, Exception> _formatterSelected;
        private static readonly Action<ILogger, string, Exception> _skippedContentNegotiation;
        private static readonly Action<ILogger, Exception> _noAcceptForNegotiation;
        private static readonly Action<ILogger, IEnumerable<MediaTypeSegmentWithQuality>, Exception> _noFormatterFromNegotiation;

        private static readonly Action<ILogger, IInputFormatter, string, Exception> _inputFormatterSelected;
        private static readonly Action<ILogger, IInputFormatter, string, Exception> _inputFormatterRejected;
        private static readonly Action<ILogger, string, Exception> _noInputFormatterSelected;
        private static readonly Action<ILogger, string, string, Exception> _removeFromBodyAttribute;

        private static readonly Action<ILogger, string, Exception> _redirectResultExecuting;

        private static readonly Action<ILogger, string, Exception> _redirectToActionResultExecuting;

        private static readonly Action<ILogger, string, string, Exception> _redirectToRouteResultExecuting;

        private static readonly Action<ILogger, string[], Exception> _noActionsMatched;

        private static readonly Action<ILogger, string, Exception> _redirectToPageResultExecuting;

        private static readonly Action<ILogger, Exception> _featureNotFound;
        private static readonly Action<ILogger, Exception> _featureIsReadOnly;
        private static readonly Action<ILogger, string, Exception> _maxRequestBodySizeSet;
        private static readonly Action<ILogger, Exception> _requestBodySizeLimitDisabled;

        private static readonly Action<ILogger, Exception> _cannotApplyRequestFormLimits;
        private static readonly Action<ILogger, Exception> _appliedRequestFormLimits;

        private static readonly Action<ILogger, Exception> _modelStateInvalidFilterExecuting;

        private static readonly Action<ILogger, MethodInfo, string, string, Exception> _inferredParameterSource;
        private static readonly Action<ILogger, MethodInfo, Exception> _unableToInferParameterSources;
        private static readonly Action<ILogger, IModelBinderProvider[], Exception> _registeredModelBinderProviders;
        private static readonly Action<ILogger, string, string, Type, Type, Exception> _foundNoValueForPropertyOnRequest;
        private static readonly Action<ILogger, string, string, Type, Exception> _foundNoValueForParameterOnRequest;
        private static readonly Action<ILogger, string, Type, Exception> _noPublicSettableProperties;
        private static readonly Action<ILogger, Type, Exception> _cannotBindToComplexType;
        private static readonly Action<ILogger, string, Type, Exception> _cannotBindToFilesCollectionDueToInvalidContentType;
        private static readonly Action<ILogger, Type, Exception> _cannotCreateHeaderModelBinder;
        private static readonly Action<ILogger, Exception> _noFilesFoundOnTheRequest;
        private static readonly Action<ILogger, string, string, Exception> _noNonIndexBasedFormatFoundForCollection;
        private static readonly Action<ILogger, string, string, string, string, string, string, Exception> _tryingToBindCollectionUsingIndexes;
        private static readonly Action<ILogger, string, string, string, string, string, string, Exception> _tryingToBindCollectionOfKeyValuePair;
        private static readonly Action<ILogger, string, Type, Type, string, Exception> _tryingToBindProperty;
        private static readonly Action<ILogger, Type, Exception> _tryingToBindModel;
        private static readonly Action<ILogger, Type, Exception> _doneTryingToBindModel;
        private static readonly Action<ILogger, string, string, Type, Exception> _tryingToBindParameter;
        private static readonly Action<ILogger, string, string, Type, Exception> _doneTryingToBindParameter;
        private static readonly Action<ILogger, string, string, Type, Exception> _tryingToValidateParameter;
        private static readonly Action<ILogger, string, string, Type, Exception> _doneTryingToValidateParameter;
        private static readonly Action<ILogger, string, Exception> _unsupportedFormatFilterContentType;
        private static readonly Action<ILogger, string, MediaTypeCollection, Exception> _actionDoesNotSupportFormatFilterContentType;
        private static readonly Action<ILogger, string, Exception> _cannotApplyFormatFilterContentType;
        private static readonly Action<ILogger, Exception> _actionDoesNotExplicitlySpecifyContentTypes;
        private static readonly Action<ILogger, IEnumerable<MediaTypeSegmentWithQuality>, Exception> _selectingOutputFormatterUsingAcceptHeader;
        private static readonly Action<ILogger, IEnumerable<MediaTypeSegmentWithQuality>, MediaTypeCollection, Exception> _selectingOutputFormatterUsingAcceptHeaderAndExplicitContentTypes;
        private static readonly Action<ILogger, Exception> _selectingOutputFormatterWithoutUsingContentTypes;
        private static readonly Action<ILogger, MediaTypeCollection, Exception> _selectingOutputFormatterUsingContentTypes;
        private static readonly Action<ILogger, Exception> _selectingFirstCanWriteFormatter;
        private static readonly Action<ILogger, Type, Type, Type, Exception> _notMostEffectiveFilter;
        private static readonly Action<ILogger, IEnumerable<IOutputFormatter>, Exception> _registeredOutputFormatters;

        static MvcCoreLoggerExtensions()
        {
            _actionExecuting = LoggerMessage.Define<string>(
                LogLevel.Debug,
                1,
                "Executing action {ActionName}");

            _actionExecuted = LoggerMessage.Define<string, double>(
                LogLevel.Information,
                2,
                "Executed action {ActionName} in {ElapsedMilliseconds}ms");

            _challengeResultExecuting = LoggerMessage.Define<string[]>(
                LogLevel.Information,
                1,
                "Executing ChallengeResult with authentication schemes ({Schemes}).");

            _contentResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing ContentResult with HTTP Response ContentType of {ContentType}");

            _actionMethodExecuting = LoggerMessage.Define<string, string[], ModelValidationState>(
                LogLevel.Information,
                1,
                "Executing action method {ActionName} with arguments ({Arguments}) - ModelState is {ValidationState}");

            _actionMethodExecuted = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                2,
                "Executed action method {ActionName}, returned result {ActionResult}.");

            _logFilterExecutionPlan = LoggerMessage.Define<string, string[]>(
                LogLevel.Debug,
                1,
                "Execution plan of {FilterType} filters (in the following order): {Filters}");

            _beforeExecutingMethodOnFilter = LoggerMessage.Define<string, string, Type>(
                LogLevel.Trace,
                2,
                "{FilterType}: Before executing {Method} on filter {Filter}.");

            _afterExecutingMethodOnFilter = LoggerMessage.Define<string, string, Type>(
                LogLevel.Trace,
                3,
                "{FilterType}: After executing {Method} on filter {Filter}.");

            _beforeExecutingActionResult = LoggerMessage.Define<Type>(
                LogLevel.Trace,
                4,
                "Before executing action result {ActionResult}.");

            _afterExecutingActionResult = LoggerMessage.Define<Type>(
                LogLevel.Trace,
                5,
                "After executing action result {ActionResult}.");

            _ambiguousActions = LoggerMessage.Define<string>(
                LogLevel.Error,
                1,
                "Request matched multiple actions resulting in ambiguity. Matching actions: {AmbiguousActions}");

            _constraintMismatch = LoggerMessage.Define<string, string, IActionConstraint>(
                LogLevel.Debug,
                2,
                "Action '{ActionName}' with id '{ActionId}' did not match the constraint '{ActionConstraint}'");

            _fileResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing FileResult, sending file as {FileDownloadName}");

            _authorizationFailure = LoggerMessage.Define<object>(
                LogLevel.Information,
                3,
                "Authorization failed for the request at filter '{AuthorizationFilter}'.");

            _resourceFilterShortCircuit = LoggerMessage.Define<object>(
                LogLevel.Debug,
                4,
                "Request was short circuited at resource filter '{ResourceFilter}'.");

            _resultFilterShortCircuit = LoggerMessage.Define<object>(
                LogLevel.Debug,
                5,
                "Request was short circuited at result filter '{ResultFilter}'.");

            _actionFilterShortCircuit = LoggerMessage.Define<object>(
                LogLevel.Debug,
                3,
                "Request was short circuited at action filter '{ActionFilter}'.");

            _exceptionFilterShortCircuit = LoggerMessage.Define<object>(
                LogLevel.Debug,
                4,
                "Request was short circuited at exception filter '{ExceptionFilter}'.");

            _forbidResultExecuting = LoggerMessage.Define<string[]>(
                LogLevel.Information,
                eventId: 1,
                formatString: $"Executing {nameof(ForbidResult)} with authentication schemes ({{Schemes}}).");

            _signInResultExecuting = LoggerMessage.Define<string, ClaimsPrincipal>(
                LogLevel.Information,
                eventId: 1,
                formatString: $"Executing {nameof(SignInResult)} with authentication scheme ({{Scheme}}) and the following principal: {{Principal}}.");

            _signOutResultExecuting = LoggerMessage.Define<string[]>(
                LogLevel.Information,
                eventId: 1,
                formatString: $"Executing {nameof(SignOutResult)} with authentication schemes ({{Schemes}}).");

            _httpStatusCodeResultExecuting = LoggerMessage.Define<int>(
                LogLevel.Information,
                1,
                "Executing HttpStatusCodeResult, setting HTTP status code {StatusCode}");

            _localRedirectResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing LocalRedirectResult, redirecting to {Destination}.");

            _noFormatter = LoggerMessage.Define<string>(
                LogLevel.Warning,
                1,
                "No output formatter was found for content type '{ContentType}' to write the response.");

            _objectResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing ObjectResult, writing value of type '{Type}'.");

            _formatterSelected = LoggerMessage.Define<IOutputFormatter, string>(
                LogLevel.Debug,
                2,
                "Selected output formatter '{OutputFormatter}' and content type '{ContentType}' to write the response.");

            _skippedContentNegotiation = LoggerMessage.Define<string>(
                LogLevel.Debug,
                3,
                "Skipped content negotiation as content type '{ContentType}' is explicitly set for the response.");

            _noAcceptForNegotiation = LoggerMessage.Define(
                LogLevel.Debug,
                4,
                "No information found on request to perform content negotiation.");

            _noFormatterFromNegotiation = LoggerMessage.Define<IEnumerable<MediaTypeSegmentWithQuality>>(
                LogLevel.Debug,
                5,
                "Could not find an output formatter based on content negotiation. Accepted types were ({AcceptTypes})");

            _inputFormatterSelected = LoggerMessage.Define<IInputFormatter, string>(
                LogLevel.Debug,
                1,
                "Selected input formatter '{InputFormatter}' for content type '{ContentType}'.");

            _inputFormatterRejected = LoggerMessage.Define<IInputFormatter, string>(
                LogLevel.Debug,
                2,
                "Rejected input formatter '{InputFormatter}' for content type '{ContentType}'.");

            _noInputFormatterSelected = LoggerMessage.Define<string>(
                LogLevel.Debug,
                3,
                "No input formatter was found to support the content type '{ContentType}' for use with the [FromBody] attribute.");

            _removeFromBodyAttribute = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                4,
                "To use model binding, remove the [FromBody] attribute from the property or parameter named '{ModelName}' with model type '{ModelType}'.");

            _redirectResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing RedirectResult, redirecting to {Destination}.");

            _redirectToActionResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing RedirectResult, redirecting to {Destination}.");

            _redirectToRouteResultExecuting = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                1,
                "Executing RedirectToRouteResult, redirecting to {Destination} from route {RouteName}.");

            _redirectToPageResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing RedirectToPageResult, redirecting to {Page}.");

            _noActionsMatched = LoggerMessage.Define<string[]>(
                LogLevel.Debug,
                3,
                "No actions matched the current request. Route values: {RouteValues}");

            _featureNotFound = LoggerMessage.Define(
                LogLevel.Warning,
                1,
                "A request body size limit could not be applied. This server does not support the IHttpRequestBodySizeFeature.");

            _featureIsReadOnly = LoggerMessage.Define(
                LogLevel.Warning,
                2,
                "A request body size limit could not be applied. The IHttpRequestBodySizeFeature for the server is read-only.");

            _maxRequestBodySizeSet = LoggerMessage.Define<string>(
                LogLevel.Debug,
                3,
                "The maximum request body size has been set to {RequestSize}.");

            _requestBodySizeLimitDisabled = LoggerMessage.Define(
                LogLevel.Debug,
                3,
                "The request body size limit has been disabled.");

            _cannotApplyRequestFormLimits = LoggerMessage.Define(
                LogLevel.Warning,
                1,
                "Unable to apply configured form options since the request form has already been read.");

            _appliedRequestFormLimits = LoggerMessage.Define(
                LogLevel.Debug,
                2,
                "Applied the configured form options on the current request.");

            _modelStateInvalidFilterExecuting = LoggerMessage.Define(
                LogLevel.Debug,
                1,
                "The request has model state errors, returning an error response.");

            _inferredParameterSource = LoggerMessage.Define<MethodInfo, string, string>(
                LogLevel.Debug,
                1,
                "Inferred binding source for '{ParameterName}` on `{ActionName}` as {BindingSource}.");

            _unableToInferParameterSources = LoggerMessage.Define<MethodInfo>(
                LogLevel.Warning,
                2,
                "Unable to unambiguously infer binding sources for parameters on '{ActionName}'. More than one parameter may be inferred to bound from body.");

            _unsupportedFormatFilterContentType = LoggerMessage.Define<string>(
                LogLevel.Debug,
                1,
                "Could not find a media type for the format '{FormatFilterContentType}'.");

            _actionDoesNotSupportFormatFilterContentType = LoggerMessage.Define<string, MediaTypeCollection>(
                LogLevel.Debug,
                2,
                "Current action does not support the content type '{FormatFilterContentType}'. The supported content types are '{SupportedMediaTypes}'.");

            _cannotApplyFormatFilterContentType = LoggerMessage.Define<string>(
                LogLevel.Debug,
                3,
                "Cannot apply content type '{FormatFilterContentType}' to the response as current action had explicitly set a preferred content type.");

            _notMostEffectiveFilter = LoggerMessage.Define<Type, Type, Type>(
                LogLevel.Debug,
                4,
                "Execution of filter {OverriddenFilter} is preempted by filter {OverridingFilter} which is the most effective filter implementing policy {FilterPolicy}.");

            _actionDoesNotExplicitlySpecifyContentTypes = LoggerMessage.Define(
                LogLevel.Debug,
                5,
                "Current action does not explicitly specify any content types for the response.");

            _selectingOutputFormatterUsingAcceptHeader = LoggerMessage.Define<IEnumerable<MediaTypeSegmentWithQuality>>(
                LogLevel.Debug,
                6,
                "Attempting to select an output formatter based on Accept header '{AcceptHeader}'.");

            _selectingOutputFormatterUsingAcceptHeaderAndExplicitContentTypes = LoggerMessage.Define<IEnumerable<MediaTypeSegmentWithQuality>, MediaTypeCollection>(
                LogLevel.Debug,
                7,
                "Attempting to select an output formatter based on Accept header '{AcceptHeader}' and explicitly specified content types '{ExplicitContentTypes}'. The content types in the accept header must be a subset of the explicitly set content types.");

            _selectingOutputFormatterWithoutUsingContentTypes = LoggerMessage.Define(
                LogLevel.Debug,
                8,
                "Attempting to select an output formatter without using a content type as no explicit content types were specified for the response.");

            _selectingOutputFormatterUsingContentTypes = LoggerMessage.Define<MediaTypeCollection>(
                LogLevel.Debug,
                9,
                "Attempting to select the first output formatter in the output formatters list which supports a content type from the explicitly specified content types '{ExplicitContentTypes}'.");

            _selectingFirstCanWriteFormatter = LoggerMessage.Define(
                LogLevel.Debug,
                10,
                "Attempting to select the first formatter in the output formatters list which can write the result.");

            _registeredOutputFormatters = LoggerMessage.Define<IEnumerable<IOutputFormatter>>(
                LogLevel.Debug,
                11,
                "List of registered output formatters, in the following order: {OutputFormatters}");

            _registeredModelBinderProviders = LoggerMessage.Define<IModelBinderProvider[]>(
                LogLevel.Debug,
                6,
                "Registered model binder providers, in the following order: {ModelBinderProviders}");

            _tryingToBindProperty = LoggerMessage.Define<string, Type, Type, string>(
               LogLevel.Debug,
               7,
               "Trying to bind property '{PropertyName}'[{ModelType}] on type '{PropertyContainerType}' using the name '{ModelName}' in request data ...");

            _foundNoValueForPropertyOnRequest = LoggerMessage.Define<string, string, Type, Type>(
               LogLevel.Debug,
               8,
               "Could not find a value on the request with name '{ModelName}' for binding property '{ModelFieldName}'[{ModelType}] on type '{PropertyContainerType}'.");

            _foundNoValueForParameterOnRequest = LoggerMessage.Define<string, string, Type>(
               LogLevel.Debug,
               9,
               "Could not find a value on the request with name '{ModelName}' for binding parameter '{ModelFieldName}'[{ModelType}].");

            _noPublicSettableProperties = LoggerMessage.Define<string, Type>(
               LogLevel.Debug,
               10,
               "Could not bind to model with name '{ModelName}' and type '{ModelType}' as there were no public settable properties on it.");

            _cannotBindToComplexType = LoggerMessage.Define<Type>(
               LogLevel.Debug,
               11,
               "Could not bind to model of type '{ModelType}' as there were no values for any of the properties on the request to bind from.");

            _cannotBindToFilesCollectionDueToInvalidContentType = LoggerMessage.Define<string, Type>(
               LogLevel.Debug,
               12,
               "Could not bind to model with name '{ModelName}' having the model type '{ModelType}' as the request did not have content type of either 'application/x-www-form-urlencoded' or 'multipart/form-data'.");

            _cannotCreateHeaderModelBinder = LoggerMessage.Define<Type>(
               LogLevel.Debug,
               13,
               "Could not create binder to bind model with type '{ModelType}' as this binder only supports 'System.String' type or collection of 'System.String' type.");

            _noFilesFoundOnTheRequest = LoggerMessage.Define(
                LogLevel.Debug,
                14,
                "No files found on the request to bind the model to.");

            _tryingToBindParameter = LoggerMessage.Define<string, string, Type>(
                LogLevel.Debug,
                15,
                "Trying to bind {ParameterOrProperty} '{ParameterOrPropertyName}' of type '{ModelType}' ...");

            _doneTryingToBindParameter = LoggerMessage.Define<string, string, Type>(
                LogLevel.Debug,
                16,
                "Done trying to bind {ParameterOrProperty} '{ParameterOrPropertyName}' of type '{ModelType}'.");

            _tryingToBindModel = LoggerMessage.Define<Type>(
                LogLevel.Debug,
                17,
                "Trying to bind model of type '{ModelType}' ...");

            _doneTryingToBindModel = LoggerMessage.Define<Type>(
                LogLevel.Debug,
                18,
                "Done trying to bind model of type '{ModelType}'.");

            _tryingToValidateParameter = LoggerMessage.Define<string, string, Type>(
                LogLevel.Debug,
                19,
                "Trying to validate the bound {ParameterOrProperty} '{ParameterOrPropertyName}' of type '{ModelType}' ...");

            _doneTryingToValidateParameter = LoggerMessage.Define<string, string, Type>(
                LogLevel.Debug,
                20,
                "Done trying to validate the bound {ParameterOrProperty} '{ParameterOrPropertyName}' of type '{ModelType}'.");

            _noNonIndexBasedFormatFoundForCollection = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                21,
                "Could not bind to collection using the format like {ModelName}=value1&{ModelName}=value2 etc.");

            _tryingToBindCollectionUsingIndexes = LoggerMessage.Define<string, string, string, string, string, string>(
                LogLevel.Debug,
                22,
                "Trying to bind model using indexes. Example formats being looked for: " +
                "[0]=value1&[1]=value2, " +
                "{ModelName}[0]=value1&{ModelName}[1]=value2, " +
                "{ModelName}.index=zero&{ModelName}.index=one&{ModelName}[zero]=value1&{ModelName}[one]=value2");

            _tryingToBindCollectionOfKeyValuePair = LoggerMessage.Define<string, string, string, string, string, string>(
                LogLevel.Debug,
                23,
                "Trying to bind collection of keyvaluepair. Example formats being looked for: " +
                "[0].Key=key1&[0].Value=value1&[1].Key=key2&[1].Value=value2, " +
                "{ModelName}[0].Key=key1&{ModelName}[0].Value=value1&{ModelName}[1].Key=key2&{ModelName}[1].Value=value2, " +
                "{ModelName}[key1]=value1&{ModelName}[key2]=value2");
        }

        public static void RegisteredOutputFormatters(this ILogger logger, IEnumerable<IOutputFormatter> outputFormatters)
        {
            _registeredOutputFormatters(logger, outputFormatters, null);
        }

        public static void SelectingOutputFormatterUsingAcceptHeaderAndExplicitContentTypes(
            this ILogger logger, 
            IEnumerable<MediaTypeSegmentWithQuality> acceptHeader, 
            MediaTypeCollection mediaTypeCollection)
        {
            _selectingOutputFormatterUsingAcceptHeaderAndExplicitContentTypes(logger, acceptHeader, mediaTypeCollection, null);
        }

        public static void SelectingOutputFormatterUsingAcceptHeader(this ILogger logger, IEnumerable<MediaTypeSegmentWithQuality> acceptHeader)
        {
            _selectingOutputFormatterUsingAcceptHeader(logger, acceptHeader, null);
        }

        public static void SelectingOutputFormatterUsingContentTypes(this ILogger logger, MediaTypeCollection mediaTypeCollection)
        {
            _selectingOutputFormatterUsingContentTypes(logger, mediaTypeCollection, null);
        }

        public static void SelectingOutputFormatterWithoutUsingContentTypes(this ILogger logger)
        {
            _selectingOutputFormatterWithoutUsingContentTypes(logger, null);
        }

        public static void SelectFirstCanWriteFormatter(this ILogger logger)
        {
            _selectingFirstCanWriteFormatter(logger, null);
        }

        public static IDisposable ActionScope(this ILogger logger, ActionDescriptor action)
        {
            return logger.BeginScope(new ActionLogScope(action));
        }

        public static void ExecutingAction(this ILogger logger, ActionDescriptor action)
        {
            _actionExecuting(logger, action.DisplayName, null);
        }

        public static void AuthorizationFiltersExecutionPlan(this ILogger logger, IEnumerable<IFilterMetadata> filters)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var authorizationFilters = filters.Where(f => f is IAuthorizationFilter || f is IAsyncAuthorizationFilter);
            LogFilterExecutionPlan(logger, "authorization", authorizationFilters);
        }

        public static void ResourceFiltersExecutionPlan(this ILogger logger, IEnumerable<IFilterMetadata> filters)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var resourceFilters = filters.Where(f => f is IResourceFilter || f is IAsyncResourceFilter);
            LogFilterExecutionPlan(logger, "resource", resourceFilters);
        }

        public static void ActionFiltersExecutionPlan(this ILogger logger, IEnumerable<IFilterMetadata> filters)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var actionFilters = filters.Where(f => f is IActionFilter || f is IAsyncActionFilter);
            LogFilterExecutionPlan(logger, "action", actionFilters);
        }

        public static void ExceptionFiltersExecutionPlan(this ILogger logger, IEnumerable<IFilterMetadata> filters)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var exceptionFilters = filters.Where(f => f is IExceptionFilter || f is IAsyncExceptionFilter);
            LogFilterExecutionPlan(logger, "exception", exceptionFilters);
        }

        public static void ResultFiltersExecutionPlan(this ILogger logger, IEnumerable<IFilterMetadata> filters)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var resultFilters = filters.Where(f => f is IResultFilter || f is IAsyncResultFilter);
            LogFilterExecutionPlan(logger, "result", resultFilters);
        }

        public static void BeforeExecutingMethodOnFilter(
            this ILogger logger,
            string filterType,
            string methodName,
            IFilterMetadata filter)
        {
            _beforeExecutingMethodOnFilter(logger, filterType, methodName, filter.GetType(), null);
        }

        public static void AfterExecutingMethodOnFilter(
            this ILogger logger,
            string filterType,
            string methodName,
            IFilterMetadata filter)
        {
            _afterExecutingMethodOnFilter(logger, filterType, methodName, filter.GetType(), null);
        }

        public static void ExecutedAction(this ILogger logger, ActionDescriptor action, long startTimestamp)
        {
            // Don't log if logging wasn't enabled at start of request as time will be wildly wrong.
            if (logger.IsEnabled(LogLevel.Information))
            {
                if (startTimestamp != 0)
                {
                    var currentTimestamp = Stopwatch.GetTimestamp();
                    var elapsed = new TimeSpan((long)(TimestampToTicks * (currentTimestamp - startTimestamp)));

                    _actionExecuted(logger, action.DisplayName, elapsed.TotalMilliseconds, null);
                }
            }
        }

        public static void NoActionsMatched(this ILogger logger, IDictionary<string, object> routeValueDictionary)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                string[] routeValues = null;
                if (routeValueDictionary != null)
                {
                    routeValues = routeValueDictionary
                        .Select(pair => pair.Key + "=" + Convert.ToString(pair.Value))
                        .ToArray();
                }
                _noActionsMatched(logger, routeValues, null);
            }
        }

        public static void ChallengeResultExecuting(this ILogger logger, IList<string> schemes)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _challengeResultExecuting(logger, schemes.ToArray(), null);
            }
        }

        public static void ContentResultExecuting(this ILogger logger, string contentType)
        {
            _contentResultExecuting(logger, contentType, null);
        }

        public static void BeforeExecutingActionResult(this ILogger logger, IActionResult actionResult)
        {
            _beforeExecutingActionResult(logger, actionResult.GetType(), null);
        }

        public static void AfterExecutingActionResult(this ILogger logger, IActionResult actionResult)
        {
            _afterExecutingActionResult(logger, actionResult.GetType(), null);
        }

        public static void ActionMethodExecuting(this ILogger logger, ControllerContext context, object[] arguments)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var actionName = context.ActionDescriptor.DisplayName;

                string[] convertedArguments;
                if (arguments == null)
                {
                    convertedArguments = null;
                }
                else
                {
                    convertedArguments = new string[arguments.Length];
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        convertedArguments[i] = Convert.ToString(arguments[i]);
                    }
                }

                var validationState = context.ModelState.ValidationState;

                _actionMethodExecuting(logger, actionName, convertedArguments, validationState, null);
            }
        }

        public static void ActionMethodExecuted(this ILogger logger, ControllerContext context, IActionResult result)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var actionName = context.ActionDescriptor.DisplayName;
                _actionMethodExecuted(logger, actionName, Convert.ToString(result), null);
            }
        }

        public static void AmbiguousActions(this ILogger logger, string actionNames)
        {
            _ambiguousActions(logger, actionNames, null);
        }

        public static void ConstraintMismatch(
            this ILogger logger,
            string actionName,
            string actionId,
            IActionConstraint actionConstraint)
        {
            _constraintMismatch(logger, actionName, actionId, actionConstraint, null);
        }

        public static void FileResultExecuting(this ILogger logger, string fileDownloadName)
        {
            _fileResultExecuting(logger, fileDownloadName, null);
        }

        public static void AuthorizationFailure(
            this ILogger logger,
            IFilterMetadata filter)
        {
            _authorizationFailure(logger, filter, null);
        }

        public static void ResourceFilterShortCircuited(
            this ILogger logger,
            IFilterMetadata filter)
        {
            _resourceFilterShortCircuit(logger, filter, null);
        }

        public static void ResultFilterShortCircuited(
            this ILogger logger,
            IFilterMetadata filter)
        {
            _resultFilterShortCircuit(logger, filter, null);
        }

        public static void ExceptionFilterShortCircuited(
            this ILogger logger,
            IFilterMetadata filter)
        {
            _exceptionFilterShortCircuit(logger, filter, null);
        }

        public static void ActionFilterShortCircuited(
            this ILogger logger,
            IFilterMetadata filter)
        {
            _actionFilterShortCircuit(logger, filter, null);
        }

        public static void ForbidResultExecuting(this ILogger logger, IList<string> authenticationSchemes)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _forbidResultExecuting(logger, authenticationSchemes.ToArray(), null);
            }
        }

        public static void SignInResultExecuting(this ILogger logger, string authenticationScheme, ClaimsPrincipal principal)
        {
            _signInResultExecuting(logger, authenticationScheme, principal, null);
        }

        public static void SignOutResultExecuting(this ILogger logger, IList<string> authenticationSchemes)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _signOutResultExecuting(logger, authenticationSchemes.ToArray(), null);
            }
        }

        public static void HttpStatusCodeResultExecuting(this ILogger logger, int statusCode)
        {
            _httpStatusCodeResultExecuting(logger, statusCode, null);
        }

        public static void LocalRedirectResultExecuting(this ILogger logger, string destination)
        {
            _localRedirectResultExecuting(logger, destination, null);
        }

        public static void ObjectResultExecuting(this ILogger logger, object value)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var type = value == null ? "null" : value.GetType().FullName;
                _objectResultExecuting(logger, type, null);
            }
        }

        public static void NoFormatter(
            this ILogger logger,
            OutputFormatterCanWriteContext formatterContext)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                _noFormatter(logger, Convert.ToString(formatterContext.ContentType), null);
            }
        }

        public static void FormatterSelected(
            this ILogger logger,
            IOutputFormatter outputFormatter,
            OutputFormatterCanWriteContext context)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var contentType = Convert.ToString(context.ContentType);
                _formatterSelected(logger, outputFormatter, contentType, null);
            }
        }

        public static void SkippedContentNegotiation(this ILogger logger, string contentType)
        {
            _skippedContentNegotiation(logger, contentType, null);
        }

        public static void NoAcceptForNegotiation(this ILogger logger)
        {
            _noAcceptForNegotiation(logger, null);
        }

        public static void NoFormatterFromNegotiation(this ILogger logger, IList<MediaTypeSegmentWithQuality> acceptTypes)
        {
            _noFormatterFromNegotiation(logger, acceptTypes, null);
        }

        public static void InputFormatterSelected(
           this ILogger logger,
           IInputFormatter inputFormatter,
           InputFormatterContext formatterContext)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var contentType = formatterContext.HttpContext.Request.ContentType;
                _inputFormatterSelected(logger, inputFormatter, contentType, null);
            }
        }

        public static void InputFormatterRejected(
            this ILogger logger,
            IInputFormatter inputFormatter,
            InputFormatterContext formatterContext)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var contentType = formatterContext.HttpContext.Request.ContentType;
                _inputFormatterRejected(logger, inputFormatter, contentType, null);
            }
        }

        public static void NoInputFormatterSelected(
            this ILogger logger,
            InputFormatterContext formatterContext)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var contentType = formatterContext.HttpContext.Request.ContentType;
                _noInputFormatterSelected(logger, contentType, null);
                if (formatterContext.HttpContext.Request.HasFormContentType)
                {
                    var modelType = formatterContext.ModelType.FullName;
                    var modelName = formatterContext.ModelName;
                    _removeFromBodyAttribute(logger, modelName, modelType, null);
                }
            }
        }

        public static void RedirectResultExecuting(this ILogger logger, string destination)
        {
            _redirectResultExecuting(logger, destination, null);
        }

        public static void RedirectToActionResultExecuting(this ILogger logger, string destination)
        {
            _redirectToActionResultExecuting(logger, destination, null);
        }

        public static void RedirectToRouteResultExecuting(this ILogger logger, string destination, string routeName)
        {
            _redirectToRouteResultExecuting(logger, destination, routeName, null);
        }

        public static void RedirectToPageResultExecuting(this ILogger logger, string page)
            => _redirectToPageResultExecuting(logger, page, null);

        public static void FeatureNotFound(this ILogger logger)
        {
            _featureNotFound(logger, null);
        }

        public static void FeatureIsReadOnly(this ILogger logger)
        {
            _featureIsReadOnly(logger, null);
        }

        public static void MaxRequestBodySizeSet(this ILogger logger, string requestSize)
        {
            _maxRequestBodySizeSet(logger, requestSize, null);
        }

        public static void RequestBodySizeLimitDisabled(this ILogger logger)
        {
            _requestBodySizeLimitDisabled(logger, null);
        }

        public static void CannotApplyRequestFormLimits(this ILogger logger)
        {
            _cannotApplyRequestFormLimits(logger, null);
        }

        public static void AppliedRequestFormLimits(this ILogger logger)
        {
            _appliedRequestFormLimits(logger, null);
        }

        public static void NotMostEffectiveFilter(this ILogger logger, Type overridenFilter, Type overridingFilter, Type policyType)
        {
            _notMostEffectiveFilter(logger, overridenFilter, overridingFilter, policyType, null);
        }

        public static void UnsupportedFormatFilterContentType(this ILogger logger, string format)
        {
            _unsupportedFormatFilterContentType(logger, format, null);
        }

        public static void ActionDoesNotSupportFormatFilterContentType(
            this ILogger logger,
            string format,
            MediaTypeCollection supportedMediaTypes)
        {
            _actionDoesNotSupportFormatFilterContentType(logger, format, supportedMediaTypes, null);
        }

        public static void CannotApplyFormatFilterContentType(this ILogger logger, string format)
        {
            _cannotApplyFormatFilterContentType(logger, format, null);
        }

        public static void ActionDoesNotExplicitlySpecifyContentTypes(this ILogger logger)
        {
            _actionDoesNotExplicitlySpecifyContentTypes(logger, null);
        }

        public static void ModelStateInvalidFilterExecuting(this ILogger logger) => _modelStateInvalidFilterExecuting(logger, null);

        public static void InferredParameterBindingSource(
            this ILogger logger,
            ParameterModel parameterModel,
            BindingSource bindingSource)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _inferredParameterSource(logger, parameterModel.Action.ActionMethod, parameterModel.ParameterName, bindingSource.DisplayName, null);
            }
        }

        public static void UnableToInferBindingSource(
            this ILogger logger,
            ActionModel actionModel)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                _unableToInferParameterSources(logger, actionModel.ActionMethod, null);
            }
        }

        public static void RegisteredModelBinderProviders(this ILogger logger, IModelBinderProvider[] providers)
        {
            _registeredModelBinderProviders(logger, providers, null);
        }

        public static void FoundNoValueOnRequest(this ILogger logger, ModelBindingContext bindingContext)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var modelMetadata = bindingContext.ModelMetadata;
            var isProperty = modelMetadata.ContainerType != null;
            var modelName = GetModelName(bindingContext);

            if (isProperty)
            {
                _foundNoValueForPropertyOnRequest(
                    logger,
                    modelName,
                    modelMetadata.PropertyName,
                    bindingContext.ModelType,
                    modelMetadata.ContainerType,
                    null);
            }
            else
            {
                _foundNoValueForParameterOnRequest(
                    logger,
                    modelName,
                    modelMetadata.PropertyName,
                    bindingContext.ModelType,
                    null);
            }
        }

        public static void NoPublicSettableProperties(this ILogger logger, ModelBindingContext bindingContext)
        {
            _noPublicSettableProperties(logger, GetModelName(bindingContext), bindingContext.ModelType, null);
        }

        public static void CannotBindToComplexType(this ILogger logger, ModelBindingContext bindingContext)
        {
            _cannotBindToComplexType(logger, bindingContext.ModelType, null);
        }

        public static void CannotBindToFilesCollectionDueToInvalidContentType(this ILogger logger, ModelBindingContext bindingContext)
        {
            _cannotBindToFilesCollectionDueToInvalidContentType(logger, GetModelName(bindingContext), bindingContext.ModelType, null);
        }

        public static void CannotCreateHeaderModelBinder(this ILogger logger, Type modelType)
        {
            _cannotCreateHeaderModelBinder(logger, modelType, null);
        }

        public static void NoFilesFoundOnTheRequest(this ILogger logger)
        {
            _noFilesFoundOnTheRequest(logger, null);
        }

        public static void TryingToBindModel(this ILogger logger, ModelBindingContext bindingContext)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var modelMetadata = bindingContext.ModelMetadata;
            var isProperty = modelMetadata.ContainerType != null;

            if (isProperty)
            {
                _tryingToBindProperty(
                    logger,
                    modelMetadata.PropertyName,
                    modelMetadata.ModelType,
                    modelMetadata.ContainerType,
                    GetModelName(bindingContext),
                    null);
            }
            else
            {
                _tryingToBindModel(logger, bindingContext.ModelType, null);
            }
        }

        public static void DoneTryingToBindModel(this ILogger logger, ModelBindingContext bindingContext)
        {
            _doneTryingToBindModel(logger, bindingContext.ModelType, null);
        }

        public static void TryingToBindParameter(this ILogger logger, ParameterDescriptor parameter, ModelBindingContext bindingContext)
        {
            _tryingToBindParameter(logger, GetParameterOrProperty(parameter), parameter.Name, bindingContext.ModelType, null);
        }

        public static void DoneTryingToBindParameter(this ILogger logger, ParameterDescriptor parameter, ModelBindingContext bindingContext)
        {
            _doneTryingToBindParameter(logger, GetParameterOrProperty(parameter), parameter.Name, bindingContext.ModelType, null);
        }

        public static void TryingToValidateParameter(this ILogger logger, ParameterDescriptor parameter, ModelBindingContext bindingContext)
        {
            _tryingToValidateParameter(logger, GetParameterOrProperty(parameter), parameter.Name, bindingContext.ModelType, null);
        }

        public static void DoneTryingToValidateParameter(this ILogger logger, ParameterDescriptor parameter, ModelBindingContext bindingContext)
        {
            _doneTryingToValidateParameter(logger, GetParameterOrProperty(parameter), parameter.Name, bindingContext.ModelType, null);
        }

        public static void NoNonIndexBasedFormatFoundForCollection(this ILogger logger, ModelBindingContext bindingContext)
        {
            var modelName = GetModelName(bindingContext);
            _noNonIndexBasedFormatFoundForCollection(logger, modelName, modelName, null);
        }

        public static void TryingToBindCollectionUsingIndexes(this ILogger logger, ModelBindingContext bindingContext)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var modelName = GetModelName(bindingContext);

            var enumerableType = ClosedGenericMatcher.ExtractGenericInterface(bindingContext.ModelType, typeof(IEnumerable<>));
            if (enumerableType != null)
            {
                var elementType = enumerableType.GenericTypeArguments[0];
                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition().GetTypeInfo() == typeof(KeyValuePair<,>).GetTypeInfo())
                {
                    _tryingToBindCollectionOfKeyValuePair(logger, modelName, modelName, modelName, modelName, modelName, modelName, null);
                    return;
                }
            }

            _tryingToBindCollectionUsingIndexes(logger, modelName, modelName, modelName, modelName, modelName, modelName, null);
        }

        private static string GetModelName(ModelBindingContext bindingContext)
        {
            var modelMetadata = bindingContext.ModelMetadata;
            var modelName = bindingContext.ModelName;
            if (string.IsNullOrEmpty(modelName))
            {
                modelName = modelMetadata.BinderModelName ?? modelMetadata.PropertyName;
            }
            return modelName;
        }

        private static string GetParameterOrProperty(ParameterDescriptor parameter)
        {
            var parameterOrProperty = "parameter";
            if (parameter is ControllerBoundPropertyDescriptor)
            {
                parameterOrProperty = "property";
            }
            return parameterOrProperty;
        }

        private static void LogFilterExecutionPlan(
            ILogger logger,
            string filterType,
            IEnumerable<IFilterMetadata> filters)
        {
            var filterList = _noFilters;
            if (filters.Any())
            {
                filterList = GetFilterList(filters);
            }

            _logFilterExecutionPlan(logger, filterType, filterList, null);
        }

        private static string[] GetFilterList(IEnumerable<IFilterMetadata> filters)
        {
            var filterList = new List<string>();
            foreach (var filter in filters)
            {
                if (filter is IOrderedFilter orderedFilter)
                {
                    filterList.Add($"{filter.GetType()} (Order: {orderedFilter.Order})");
                }
                else
                {
                    filterList.Add(filter.GetType().ToString());
                }
            }
            return filterList.ToArray();
        }

        private class ActionLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly ActionDescriptor _action;

            public ActionLogScope(ActionDescriptor action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                _action = action;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("ActionId", _action.Id);
                    }
                    else if (index == 1)
                    {
                        return new KeyValuePair<string, object>("ActionName", _action.DisplayName);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
            }

            public int Count => 2;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            public override string ToString()
            {
                // We don't include the _action.Id here because it's just an opaque guid, and if
                // you have text logging, you can already use the requestId for correlation.
                return _action.DisplayName;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
