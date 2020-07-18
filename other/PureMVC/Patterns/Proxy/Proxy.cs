//
//  PureMVC C# Standard
//
//  Copyright(c) 2017 Saad Shams <saad.shams@puremvc.org>
//  Your reuse is governed by the Creative Commons Attribution 3.0 License
//

using FlatBuffers;
using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Patterns.Proxy
{
    /// <summary>
    /// A base <c>IProxy</c> implementation. 
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In PureMVC, <c>Proxy</c> classes are used to manage parts of the 
    ///         application's data model.
    ///     </para>
    ///     <para>
    ///          A <c>Proxy</c> might simply manage a reference to a local data object, 
    ///          in which case interacting with it might involve setting and 
    ///          getting of its data in synchronous fashion.
    ///     </para>
    ///     <para>
    ///         <c>Proxy</c> classes are also used to encapsulate the application's 
    ///         interaction with remote services to save or retrieve data, in which case,
    ///         we adopt an asyncronous idiom; setting data (or calling a method) on the 
    ///         <c>Proxy</c> and listening for a <c>Notification</c> to be sent 
    ///         when the <c>Proxy</c> has retrieved the data from the service.
    ///     </para>
    /// </remarks>
    /// <seealso cref="PureMVC.Core.Model"/>
    public class Proxy: Notifier, IProxy, INotifier
    {
        /// <summary> Name of the proxy</summary>
        private ProxyName m_Name;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="proxyName"></param>
        public Proxy(ProxyName proxyName)
        {
            m_Name = proxyName;
        }

        /// <summary>
        /// Called by the Model when the Proxy is registered
        /// </summary>
        public virtual void OnRegister()
        { 
        }

        /// <summary>
        /// Called by the Model when the Proxy is removed
        /// </summary>
        public virtual void OnRemove()
        {
        }

        /// <summary>
        /// Called by the Model when the Data is loaded
        /// </summary>
        /// <param name="byteBuffer">数据</param>
        public virtual void InitData(ByteBuffer byteBuffer)
        {
        }

        public ProxyName Name
        {
            get
            {
                return m_Name;
            }
        }
    }
}
