﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.Core
{
    public class DependencyPropertyWatcher : IAttachedObject
    {
        private readonly List<Tuple<DependencyProperty, EventHandler>> handlers = new List<Tuple<DependencyProperty, EventHandler>>();
        private readonly Dictionary<DependencyProperty, DependencyPropertyDescriptor> descriptors = new Dictionary<DependencyProperty, DependencyPropertyDescriptor>();
        private FrameworkElement frameworkElement;

        private bool handlerRegistered;

        public DependencyPropertyWatcher()
        {
        }

        public DependencyPropertyWatcher(FrameworkElement attachTo)
        {
            Attach(attachTo);
        }

        public DependencyObject AssociatedObject { get { return frameworkElement; } }

        public void Attach(DependencyObject dependencyObject)
        {
            if (dependencyObject == null) throw new ArgumentNullException("dependencyObject");
            if (ReferenceEquals(dependencyObject, frameworkElement))
                return;

            if (frameworkElement != null)
                throw new InvalidOperationException("A dependency object is already attached to this instance of DependencyPropertyWatcher.");
            frameworkElement = dependencyObject as FrameworkElement;

            if (frameworkElement == null)
                throw new ArgumentException("The dependency object to attach to the DependencyPropertyWatcher must be a FrameworkElement.");

            frameworkElement.Loaded += ElementLoaded;
            frameworkElement.Unloaded += ElementUnloaded;
            AttachHandlers();
        }

        public void Detach()
        {
            frameworkElement.Loaded -= ElementLoaded;
            frameworkElement.Unloaded -= ElementUnloaded;
            DetachHandlers();
            handlers.Clear();
            frameworkElement = null;
        }

        public void RegisterValueChangedHandler(DependencyProperty property, EventHandler handler)
        {
            handlers.Add(Tuple.Create(property, handler));
            if (handlerRegistered)
            {
                AttachHandler(property, handler);
            }
        }

        public void UnregisterValueChangedHander(DependencyProperty property, EventHandler handler)
        {
            handlers.RemoveWhere(x => x.Item1 == property && x.Item2 == handler);
            if (handlerRegistered)
            {
                DetachHandler(property, handler);
            }
        }

        private void AttachHandlers()
        {
            if (!handlerRegistered)
            {
                foreach (var handler in handlers)
                {
                    AttachHandler(handler.Item1, handler.Item2);
                }
                handlerRegistered = true;
            }
        }

        private void DetachHandlers()
        {
            if (handlerRegistered)
            {
                foreach (var handler in handlers)
                {
                    DetachHandler(handler.Item1, handler.Item2);
                }
                handlerRegistered = false;
            }
        }

        private void AttachHandler(DependencyProperty property, EventHandler handler)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (handler == null) throw new ArgumentNullException("handler");
            if (frameworkElement == null) throw new InvalidOperationException("A dependency object must be attached in order to register a handler.");

            DependencyPropertyDescriptor descriptor;
            if (!descriptors.TryGetValue(property, out descriptor))
            {
                descriptor = DependencyPropertyDescriptor.FromProperty(property, AssociatedObject.GetType());
                descriptors.Add(property, descriptor);
            }
            descriptor.AddValueChanged(AssociatedObject, handler);
        }

        private void DetachHandler(DependencyProperty property, EventHandler handler)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (handler == null) throw new ArgumentNullException("handler");
            if (frameworkElement == null) throw new InvalidOperationException("A dependency object must be attached in order to unregister a handler.");

            DependencyPropertyDescriptor descriptor;
            if (!descriptors.TryGetValue(property, out descriptor))
            {
                throw new InvalidOperationException("No handler was previously registered for this dependency property.");
            }
            descriptor.RemoveValueChanged(AssociatedObject, handler);
        }

        private void ElementLoaded(object sender, RoutedEventArgs e)
        {
            AttachHandlers();
        }

        private void ElementUnloaded(object sender, RoutedEventArgs e)
        {
            DetachHandlers();
        }
    }
}