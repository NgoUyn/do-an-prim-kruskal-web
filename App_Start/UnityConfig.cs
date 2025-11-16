using Prim_Kruskal_Web.Models; // 2. THÊM DÒNG NÀY để nhận diện DataContext
using System;
using Unity;
using Unity.Lifetime; // 1. THÊM DÒNG NÀY để dùng PerRequestLifetimeManager
using Unity.AspNet.Mvc;

namespace Prim_Kruskal_Web
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // === ĐÂY LÀ PHẦN QUAN TRỌNG BẠN CẦN THÊM ===

            // Đăng ký DataContext với vòng đời PerRequest (Mỗi request HTTP tạo 1 cái mới)
            // Điều này sửa lỗi "No parameterless constructor" ở UngDungController
            container.RegisterType<DataContext>(new PerRequestLifetimeManager());

            // ===========================================
        }
    }
}