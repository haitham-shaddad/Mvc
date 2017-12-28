// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// Allows customization of the <see cref="PageParameterModel"/>.
    /// </summary>
    public interface IPageParameterModelConvention : IPageConvention
    {
        /// <summary>
        /// Called to apply the convention to the <see cref="PageParameterModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="PageParameterModel"/>.</param>
        void Apply(PageParameterModel model);
    }
}
