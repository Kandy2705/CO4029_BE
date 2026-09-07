using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CO4029_BE.API.Contracts.Examples;

public class ExampleSchemaFilter: ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateUserRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "email", new OpenApiString("customer@example.com") },
                { "password", new OpenApiString("<customer-password>") },
                { "name", new OpenApiString("Nguyễn Văn A") },
                { "phone", new OpenApiString("0900000000") },
                { "birthday", new OpenApiString("2025-12-11T05:33:56.193Z") },
                { "gender", new OpenApiString("Male") }
            };
        }

        // Thêm ví dụ cho LogInRequest
        if (context.Type == typeof(LoginRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "email", new OpenApiString("admin@example.com") },
                { "password", new OpenApiString("<admin-password>") }
            };
        }

        if (context.Type == typeof(LoginResponse))
        {
            schema.Example = new OpenApiObject
            {
                { "accessToken", new OpenApiString("<supabase-access-token>") },
                { "refreshToken", new OpenApiString("<supabase-refresh-token>") },
                { "expiresAt", new OpenApiString("2025-12-11T06:40:09.513311") }
            };
        }

        if (context.Type == typeof(UserReponse))
        {
            schema.Example = new OpenApiObject
            {
                { "id", new OpenApiString("user-123-abc") },
                { "email", new OpenApiString("customer@example.com") },
                { "name", new OpenApiString("Nguyễn Văn A") },
                { "phone", new OpenApiString("0900000000") },
                { "birthday", new OpenApiString("2025-12-11T05:33:56.193Z") },
                { "role", new OpenApiString("Customer") },
                { "gender", new OpenApiString("Male") },
                { "isActive", new OpenApiBoolean(true) }
            };
        }

        if (context.Type == typeof(AdminUpdateUserRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "name", new OpenApiString("Nguyễn Văn A") },
                { "phone", new OpenApiString("0900000000") },
                { "gender", new OpenApiString("Male") },
                { "role", new OpenApiString("Employee") }
            };
        }

        if (context.Type == typeof(UpdateUserStatusRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "isActive", new OpenApiBoolean(false) }
            };
        }

        if (context.Type == typeof(UpdateUserRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "name", new OpenApiString("Nguyễn Văn A") },
                { "phone", new OpenApiString("0900000000") },
                { "birthday", new OpenApiString("2025-12-11T05:33:56.193Z") },
                { "gender", new OpenApiString("Male") }
            };
        }

        if (context.Type == typeof(ChangePasswordRequest))
        {
            schema.Example = new OpenApiObject
            {
                {"email", new OpenApiString("tuduong05042003@gmail.com")},
                { "newPassword", new OpenApiString("<new-password>") },
                { "oldPassword", new OpenApiString("<old-password>") },
                {"otpCode", new OpenApiString("123456") },
            };
        }

        if (context.Type == typeof(SendOTPRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "Email", new OpenApiString("customer@example.com") }
            };
        }

        if (context.Type == typeof(CreateHistoryRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "Header", new OpenApiString("Where is B4?") }
            };
        }

        if (context.Type == typeof(HistoryReponse))
        {
            schema.Example = new OpenApiObject
            {
                { "Header", new OpenApiString("Where is B4?") },
                { "Create_date", new OpenApiString("2025-12-11T06:40:09.513311") }
            };
        }

        if (context.Type == typeof(CreateBuildingRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "Name", new OpenApiString("B4") },
                { "Content", new OpenApiString("B4 gần Circle K") },
                { "Latitude", new OpenApiFloat(14.55f) },
                { "Longitude", new OpenApiFloat(12.55f) },
                { "UserId", new OpenApiString("1231241424894893") },
            };
        }

        if (context.Type == typeof(UpdateBuildingRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "Name", new OpenApiString("B4") },
                { "Content", new OpenApiString("B4 gần Circle K") },
                { "Latitude", new OpenApiFloat(14.55f) },
                { "Longitude", new OpenApiFloat(12.55f) }
            };
        }

        if (context.Type == typeof(BuildingReponse))
        {
            schema.Example = new OpenApiObject
            {
                {"Id", new OpenApiString("31232178784398") },
                { "Name", new OpenApiString("B4") },
                { "Content", new OpenApiString("B4 gần Circle K") },
                { "Latitude", new OpenApiFloat(14.55f) },
                { "Longitude", new OpenApiFloat(12.55f) }
            };
        }

        if (context.Type == typeof(CreateChatboxRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "Content", new OpenApiString("Where is B4?") },
                { "History_id", new OpenApiString("b15e1901-4254-4527-9848-159c15f6e369")},
            };
        }

        if (context.Type == typeof(ChatboxReponse))
        {
            schema.Example = new OpenApiObject
            {
                { "Id", new OpenApiString("b15e1901-4254-4527-9848-159c15f6e369") },
                { "Content", new OpenApiString("Where is B4?") },
                { "Contact_person", new OpenApiString("user") }
            };
        }
        
        if (context.Type == typeof(CreateCategoryRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "name", new OpenApiString("Thủ tục hành chính") }
            };
        }

        if (context.Type == typeof(CategoryReponse))
        {
            schema.Example = new OpenApiObject
            {
                { "id", new OpenApiString("cat-789-xyz") },
                { "name", new OpenApiString("Thủ tục hành chính") }
            };
        }
        
        if (context.Type == typeof(CreateQuestionRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "content", new OpenApiString("Làm sao để đăng ký gửi xe tháng ở nhà B4?") },
                { "email", new OpenApiString("student@hcmut.edu.vn") },
                { "name", new OpenApiString("Nguyễn Văn A") },
                { "createDate", new OpenApiString("2026-03-23T15:00:00Z") },
                { "categoryId", new OpenApiString("cat-789-xyz") },
                { "userId", new OpenApiString("user-123-abc") }
            };
        }

        if (context.Type == typeof(QuestionReponse))
        {
            schema.Example = new OpenApiObject
            {
                { "id", new OpenApiString("ques-456-def") },
                { "content", new OpenApiString("Làm sao để đăng ký gửi xe tháng ở nhà B4?") },
                { "email", new OpenApiString("student@hcmut.edu.vn") },
                { "name", new OpenApiString("Nguyễn Văn A") },
                { "createDate", new OpenApiString("2026-03-23T15:00:00Z") },
                { "categoryId", new OpenApiString("cat-789-xyz") },
                { "userId", new OpenApiString("user-123-abc") }
            };
        }
        
        if (context.Type == typeof(CreateAnswerRequest))
        {
            schema.Example = new OpenApiObject
            {
                { "content", new OpenApiString("Bạn có thể đến văn phòng đoàn tại tầng trệt nhà B4 để làm thủ tục nhé.") },
                { "createdDate", new OpenApiString("2026-03-23T16:30:00Z") },
                { "questionId", new OpenApiString("ques-456-def") },
                { "userId", new OpenApiString("admin-999-mod") }
            };
        }

        if (context.Type == typeof(AnswerReponse))
        {
            schema.Example = new OpenApiObject
            {
                { "id", new OpenApiString("ans-111-ghi") },
                { "content", new OpenApiString("Bạn có thể đến văn phòng đoàn tại tầng trệt nhà B4 để làm thủ tục nhé.") },
                { "createDate", new OpenApiString("2026-03-23T16:30:00Z") },
                { "questionId", new OpenApiString("ques-456-def") },
                { "userId", new OpenApiString("admin-999-mod") }
            };
        }
    }
}
