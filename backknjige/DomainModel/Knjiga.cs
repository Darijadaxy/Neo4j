namespace DomainModel;
public class Knjiga
{
    public String? id { get; set; }
    public required String naslov { get; set; }

    public String? opis { get; set; }

    public required String zanr { get; set; } 
    public required String autor { get; set; } 
    public required String slika { get; set; }
 
    public List<Ocena>? ocene { get; set; }
    public int brojStranica { get; set; }

}