using DomainModel;

public class Citanje
{
    public required Korisnik korisnik { get; set; }

    public required Knjiga knjiga { get; set; }

    public required String status { get; set; }  //finished,started, in_progress
    public int trenutnaStrana { get; set; } 

}