using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class AutorController : ControllerBase
{
    private readonly IGraphClient _client;

    public AutorController(IGraphClient client)
    {
        _client = client;
    }

    [HttpPost]
    [Route("dodajAutora"), Authorize(Roles = "admin")]
    public async Task<ActionResult> DodajAutora([FromBody] Autor autor)
    {
        if (autor == null)
        {
            return BadRequest("Podaci o autoru nisu validni.");
        }

        try
        {
            await _client.Cypher
                .Create("(n:Autor {id: $id, punoIme: $punoIme, biografija: $biografija, slika: $slika})")
                .WithParams(new
                {
                    id = Guid.NewGuid().ToString(),
                    autor.punoIme,
                    autor.biografija,
                    autor.slika,
                })
                .ExecuteWithoutResultsAsync();

            return Ok("Autor je uspešno dodat.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("vratiSveAutore"), Authorize(Roles = "admin")] 
    public async Task<ActionResult> VratiSveAutore()
    {
        try
        {
            var autori = await _client.Cypher
                .Match("(a:Autor)")  
                .Return(a => a.As<Autor>())
                .ResultsAsync;

            if (autori == null || !autori.Any())
            {
                return NotFound("Nema autora u bazi.");
            }

            return Ok(autori);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("vratiSveAutoreZaBrisanje"), Authorize(Roles = "admin")]
    public async Task<ActionResult> VratiSveAutoreZaBrisanje()
    {
        try
        {
            var autori = await _client.Cypher
                .Match("(a:Autor)")
                .Where("NOT EXISTS { MATCH (:Knjiga)-[:IMA_AUTORA]->(a) }")
                .Return(a => a.As<Autor>())
                .ResultsAsync;

            if (autori == null || !autori.Any())
            {
                return Ok(new List<Autor>());
            }

            return Ok(autori);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpPut]
    [Route("azurirajAutora/{id}"), Authorize(Roles = "admin")]
    public async Task<ActionResult> AzurirajAutora(string id, [FromBody] AutorDTO autor)
    {
        if (autor == null)
        {
            return BadRequest("Podaci o autoru nisu validni.");
        }

        try
        {
            
            var autorExists = await _client.Cypher
                .Match("(a:Autor)")
                .Where((Autor a) => a.id == id)
                .Return<int>("count(a)")
                .ResultsAsync;


            if (autorExists.FirstOrDefault() == 0)
            {
                return NotFound($"Autor sa ID-em {id} nije pronađen.");
            }

            var query = _client.Cypher.Match("(a:Autor)")
                .Where((Autor a) => a.id == id);
            
            if (!string.IsNullOrEmpty(autor.punoIme))
            {
                query = query.Set("a.punoIme = $punoIme");
            }

            if (!string.IsNullOrEmpty(autor.biografija))
            {
                query = query.Set("a.biografija = $biografija");
            }

            if (!string.IsNullOrEmpty(autor.slika))
            {
                query = query.Set("a.slika = $slika");
            }

           
            var parameters = new
            {
                autor.punoIme,
                autor.biografija,
                autor.slika
            };

            
            await query.WithParams(parameters).ExecuteWithoutResultsAsync();

            return Ok("Autor je uspešno ažuriran.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("vratiAutora/{id}"), Authorize(Roles = "user")]
    public async Task<ActionResult> VratiAutora(string id)
    {
        try
        {
            var result = await _client.Cypher
                .Match("(a:Autor)")
                .Where((Autor a) => a.id == id)
                .Return(a => a.As<Autor>())
                .ResultsAsync;

            if (result == null || !result.Any())
            {
                return NotFound($"Autor sa ID-em {id} nije pronađen.");
            }

            return Ok(result.First());  
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpDelete]
    [Route("obrisiAutora/{id}"), Authorize(Roles = "admin")]
    public async Task<ActionResult> ObrisiAutora(string id) 
    {
        try
        {
            var result = await _client.Cypher
                .Match("(a:Autor)")
                .Where((Autor a) => a.id == id)
                .Return<int>("count(a)")
                .ResultsAsync;

            if (result.FirstOrDefault() == 0)
            {
                return NotFound($"Autor sa ID-em {id} nije pronađen.");
            }

           
            var imaAutoraCount = await _client.Cypher
                .Match("(kn:Knjiga)-[:IMA_AUTORA]->(a:Autor)")
                .Where((Autor a) => a.id == id)
                .Return<int>("count(kn)")
                .ResultsAsync;

          
            if (imaAutoraCount.FirstOrDefault() > 0)
            {
                Console.WriteLine($"Povezan sa knjigama: {imaAutoraCount.FirstOrDefault()}");
                return BadRequest($"Autor sa ID-jem {id} ne može biti obrisan jer je povezan sa knjigama.");
            }

            await _client.Cypher
                .Match("(a:Autor)")
                .Where((Autor a) => a.id == id)
                .Delete("a")
                .ExecuteWithoutResultsAsync();

            return Ok($"Autor sa ID-em {id} je uspešno obrisan.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("pretraziAutore/{query}"), Authorize(Roles = "user")]
    public async Task<ActionResult> PretraziAutore(string query)
    {
        try
        {
            IEnumerable<Autor> autori;

            if (string.IsNullOrWhiteSpace(query)) 
            {
                
                autori = await _client.Cypher
                    .Match("(a:Autor)")
                    .Return(a => a.As<Autor>())
                    .ResultsAsync;
            }
            else
            {
                autori = await _client.Cypher
               .Match("(a:Autor)")
               .Where("toLower(a.punoIme) CONTAINS toLower($query)")
               .WithParam("query", query)
               .Return(a => a.As<Autor>())
               .ResultsAsync;
            }

            if (!autori.Any())
            {
                return NotFound($"Nijedan autor ne odgovara upitu '{query}'.");
            }

            return Ok(autori);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }



}


