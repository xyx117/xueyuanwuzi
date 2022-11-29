using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    //public class AccountViewModels
    //{
    //}

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string Action { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "用户名不能为空！")]
        //[EmailAddress]
        //[Display(Name = "电子邮件")]
        //public string Email { get; set; }
        [Display(Name = "用户名")]
        public string Name { get; set; }

        [Required(ErrorMessage = "密码不能为空！")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "下次自动登录吗?")]
        public bool RememberMe { get; set; }

    }


    //部门负责人添加人员引用model
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }

        //[Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        //[Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        //[Display(Name = "确认密码")]
        //[Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        //[Required]
        //public string suoshuxueyuan { get; set; }

        //[Required]
        //public int parentID { get; set; }


        [Required]
        public string Zhenshiname { get; set; }

        //联系电话
        public string PhoneNumber { get; set; }

    }
    ////自定义视图模型用于创建用户
    //public class CreateModel
    //{
    //    [Required]
    //    public string Name { get; set; }

    //    [Required]
    //    public string Password { get; set; }

    //    [Required]
    //    public string suoshuxueyuan { get; set; }

    //    [Required]
    //    public int parentID { get; set; }
    //}


    //业务专员添加业务专员，部门负责人，领导，分管领导角色时使用的视图模型
    public class AdminRegisterViewModel
    {
        [Required]

        public string UserName { get; set; }

        //[Required]              //这里原本是有[required]的，后来加入输入密码可以为空后，这里注释掉
        //[StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        //[Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        //[Display(Name = "确认密码")]
        //[Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        //[Required]
        public string Suoshuxueyuan { get; set; }

        //[Required]
        public string Suoshubumen { get; set; }


        [Required]
        public string Zhenshiname { get; set; }

        [Required]
        public string Role { get; set; }

        //联系电话
        public string PhoneNumber { get; set; }

    }


    //业务专员添加评委角色时使用的视图模型，因为这里的role是直接赋值为“评委”的，与业务专员添加其他角色不同
    public class YwzyRegister_pw_ViewModel
    {
        [Required]

        public string UserName { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        //[Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        //[Display(Name = "确认密码")]
        //[Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Suoshuxueyuan { get; set; }

        //[Required]
        //public int parentID { get; set; }


        [Required]
        public string Zhenshiname { get; set; }

        //评委参数目录
        public string Pingshenmulu { get; set; }

        //[Required]
        //public string role { get; set; }

        //评委评审任务
        public string Pingshenrenwu { get; set; }

        //联系电话
        public string PhoneNumber { get; set; }
    }

    public class ResetPasswordViewModel
    {
        //[Required]
        //[EmailAddress]
        //[Display(Name = "电子邮件")]
        //public string Email { get; set; }

        [Required]
        [Display(Name = "用户名")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        //public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

}