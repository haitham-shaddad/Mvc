// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// Allows customization of the <see cref="PropertyModel"/>.
    /// </summary>
    /// <remarks>
    /// To use this interface, create an <see cref="System.Attribute"/> class which implements the interface and
    /// place it on a controller property.
    ///
    /// <see cref="IPropertyModelConvention"/> customizations run after
    /// <see cref="IControllerModelConvention"/> customizations.
    /// </remarks>
    public interface IPropertyModelConvention
    {
        /// <summary>
        /// Called to apply the convention to the <see cref="PropertyModel"/>.
        /// </summary>
        /// <param name="parameter">The <see cref="PropertyModel"/>.</param>
        void Apply(PropertyModel parameter);
    }
}
