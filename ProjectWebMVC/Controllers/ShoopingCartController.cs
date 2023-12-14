﻿using Microsoft.AspNetCore.Mvc;
using ProjectWebMVC.Models;
using ProjectWebMVC.Services;

namespace ProjectWebMVC.Controllers
{
    public class ShoopingCartController : Controller
    {
        private readonly IVnPayService _vnPayService;

        public ShoopingCartController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CreatePaymentUrl(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }
    }
}
