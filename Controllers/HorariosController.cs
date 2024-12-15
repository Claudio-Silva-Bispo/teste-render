using Project.Infrastructure.Interfaces;
using Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("Horarios")] 
public class HorariosController : Controller
{
    private readonly IHorariosService _horariosService;

    public HorariosController(IHorariosService horariosService)
    {
        _horariosService = horariosService;
    }

    [AllowAnonymous]
    [HttpGet("Criar")]
    public IActionResult Criar()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost("Criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(HorariosDTO horariosDTO)
    {

        if (ModelState.IsValid)
        {

            var idUsuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized("Usuário não logado.");
            }

            var horario = new Horarios
            {
                HorariosPreferencia = horariosDTO.HorariosPreferencia,
                IdUsuario = idUsuario
            };

            await _horariosService.Criar(horario);

            TempData["SuccessMessage"] = "Preferencia de horário cadastrado com sucesso!";
           
        }
        return View(horariosDTO);
    }

    [HttpGet("Mensagem")]
    public IActionResult Mensagem()
    {
        return View();
    }

    [HttpGet("Consultar")]
    public async Task<IActionResult> Consultar()
    {
        var horarios = await _horariosService.ConsultarTodos(); 
        return View(horarios); 
    }

    [HttpGet("Atualizar")]
    public async Task<IActionResult> Atualizar()
    {
        //var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Error");
        }

        var horario = await _horariosService.ConsultarPorUsuarioId(userIdString);
        if (horario == null)
        {
            return NotFound();
        }

        return View(horario);
    }

    [HttpPost("Atualizar")]
    public async Task<IActionResult> Atualizar(Horarios horario)
    {
        if (!ModelState.IsValid)
        {
            return View(horario);
        }

        //var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Error");
        }

        var horarioExistente = await _horariosService.ConsultarPorUsuarioId(userIdString);

        if (horarioExistente == null)
        {
            return NotFound();
        }

        horarioExistente.HorariosPreferencia = horario.HorariosPreferencia;
       

        await _horariosService.Atualizar(horarioExistente);

        TempData["SuccessMessage"] = "Dado atualizado com sucesso!";
        return RedirectToAction("Consultar");
    }


    [HttpPost("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(string idDia)
    {
        Console.WriteLine($"Excluindo Horário com Id: {idDia}");

        try
        {
            await _horariosService.Excluir(idDia);
            TempData["SuccessMessage"] = "Exclusão realizada com sucesso!";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = $"Erro: {ex.Message}";
        }

        return RedirectToAction("Consultar");
    }

}