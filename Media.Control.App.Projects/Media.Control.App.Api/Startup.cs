using Media.Control.App.Api.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Media.Control.App.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Controller 등록
            services.AddControllers();

            // Swagger 설정
            services.AddSwaggerGen();

            // CORS 정책 추가
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // HTTPS 리디렉션 설정
            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 5050; // HTTPS 포트 설정
            });

            // SignalR 서비스 등록
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 개발 환경에서 예외 페이지 활성화
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Swagger 설정
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

            // HTTPS 리디렉션 활성화
            app.UseHttpsRedirection();

            // 라우팅 설정
            app.UseRouting();

            // CORS 활성화
            app.UseCors("AllowAll");

            // 권한 부여 미들웨어
            app.UseAuthorization();

            // 엔드포인트 매핑
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // 컨트롤러 매핑
                endpoints.MapHub<LogHub>("/loghub"); // SignalR Hub 경로 추가
                endpoints.MapHub<MediaHub>("/mediahub"); // SignalR Hub 경로 추가
            });
        }
    }
}
