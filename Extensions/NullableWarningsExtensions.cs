using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TiengAnh.Extensions
{
    public static class NullableWarningsExtensions
    {
        public static IServiceCollection SuppressNullableWarnings(this IServiceCollection services)
        {
            // Phương thức này không làm gì cả - chỉ để giúp loại bỏ cảnh báo nullable trong dự án
            // Thực tế, các cảnh báo nullable nên được giải quyết bằng cách sửa đổi code
            return services;
        }
        
        public static WebApplication SuppressNullableWarnings(this WebApplication app)
        {
            // Tương tự, phương thức này chỉ để đánh dấu
            return app;
        }
    }
}
