namespace DomainModel;
public class Korisnik
{
    public String? id { get; set; }
    public required String ime { get; set; }

    public required String username { get; set; }

    public required String sifra { get; set; }

    public String? opis { get; set; }

 
   public List<String>? zanrovi { get; set; } //id

    public String? slika { get; set; }

    public List<Korisnik>? prijatelji { get; set; }  

  



}