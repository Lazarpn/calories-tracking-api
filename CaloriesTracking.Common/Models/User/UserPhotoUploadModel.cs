using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Models.User;
public class UserPhotoUploadModel
{
    public IFormFile File { get; set; }
}
