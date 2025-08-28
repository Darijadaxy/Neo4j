import React, { useContext, useState, useEffect } from "react";
import UserProfile from "../komponente/UserProfile.js";
import AzuriranjeProfila from '../stranice/AzuriranjeProfila.js';
import TrenutnaKnjiga from '../stranice/TrenutnaKnjiga.js';
import { AppContext } from "../App"; 
import { useNavigate } from "react-router-dom"; 
import Header from "../komponente/Header.js";

const ProfilnaStrana = () => {
  const navigate = useNavigate();
  const { korisnik, loading } = useContext(AppContext);
  const [showProfileUpdate, setShowProfileUpdate] = useState(false);
  const [showTrenutna, setShowTrenutna] = useState(false);
  const [currentBook, setCurrentBook] = useState(null); 
  const [knjige, setKnjige] = useState([]); 
  const [procitaneKnjige, setProcitaneKnjige] = useState([]); 
  const [currentPage, setCurrentPage] = useState(0);
  const [activeTab, setActiveTab] = useState("current"); 


  useEffect(() => {
    const fetchKnjige = async () => {
      try {
        const token = sessionStorage.getItem("jwt");
        const response = await fetch("http://localhost:5108/Korisnik/vratiKnjigeKojeCita", {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });
        if (!response.ok) {
          throw new Error("Greška pri dohvatanju knjiga.");
        }
        const data = await response.json();
        setKnjige(data);
      } catch (error) {
        console.error("Došlo je do greške:", error.message);
      }
    };

    const fetchProcitaneKnjige = async () => {
      try {
        const token = sessionStorage.getItem("jwt");
        const response = await fetch("http://localhost:5108/Korisnik/vratiProcitane", {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });
        if (!response.ok) {
          throw new Error("Greška pri dohvatanju pročitanih knjiga.");
        }
        const data = await response.json();
        setProcitaneKnjige(data);
      } catch (error) {
        console.error("Došlo je do greške:", error.message);
      }
    };

    if (korisnik) {
      fetchKnjige();
      fetchProcitaneKnjige();
    }
  }, [korisnik]);

  const handleDeleteProfile = async () => {
    if (!window.confirm("Da li ste sigurni da želite da obrišete profil?")) return;

    try {
      const token = sessionStorage.getItem("jwt");
      const response = await fetch("http://localhost:5108/Korisnik/obrisiProfil", {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token} `},
      });

      if (!response.ok) throw new Error("Greška pri brisanju profila.");

      sessionStorage.removeItem("jwt");
      navigate("/");
    } catch (error) {
      console.error("Greška:", error.message);
    }
  };

  if (loading) {
    return <div className="min-h-screen flex items-center justify-center text-blue-600">Učitavanje...</div>;
  }

  if (!korisnik) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        Loading
      </div>
    );
  }

  
  const knjigeZaPrikaz = activeTab === "current" ? knjige.slice(currentPage * 5, currentPage * 5 + 5) : procitaneKnjige.slice(currentPage * 5, currentPage * 5 + 5);

  return (
    <div className="min-h-screen bg-gray-100 bg-gradient-to-r from-blue-30 to-green-30 flex flex-col items-center justify-center space-y-6 p-4">
      <Header/>
      <UserProfile ime={korisnik.ime} opis={korisnik.opis} slika={korisnik.slika} />
      
      <div className="flex space-x-4">
        <button
          onClick={() => setShowProfileUpdate(true)}
          className="px-4 py-2 bg-[rgb(224,209,190)] text-black rounded-lg hover:bg-[rgb(218,200,180)] transition-all transform hover:scale-105"
        >
          Update Profile
        </button>

        <button
          onClick={handleDeleteProfile}
          className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-500 transition-all transform hover:scale-105"
        >
          Delete Profile
        </button>
      </div>


      
      <div className="flex space-x-4 border-b">
        <button
          onClick={() => setActiveTab("current")}
          className={`px-4 py-2 ${
            activeTab === "current"
              ? "border-b-2 border-yellow-900 font-bold" 
              : ""
          }`}
        >
          Currently Reading 
        </button>
        <button
          onClick={() => setActiveTab("read")}
          className={`px-4 py-2  ${
            activeTab === "read"
              ? "border-b-2 border-yellow-900 font-bold"
              : ""
          }`}
        >
          Read Books
        </button>
      </div>

      
      <div className="w-full   max-w-4xl space-y-4 mt-6">
        <h2 className="text-2xl font-bold text-black">{activeTab === "current" ? "Currently Reading:" : "Read Books:"}</h2>
        <div className="relative flex items-center">
          
          {currentPage > 0 && (
            <button
              onClick={() => setCurrentPage((prev) => prev - 1)}
              className="absolute left-0 p-2 bg-[rgb(224,209,190)] hover:bg-[rgb(218,200,180)] rounded-full transition-all"
            >
              ◀
            </button>
          )}

         
          {knjigeZaPrikaz.length === 0 ? (
            <p className="text-left text-gray-500 w-full">No books to display</p>
          ) : (
            <div className="flex overflow-hidden space-x-4 mx-12">
              {knjigeZaPrikaz.map((k, index) => (
                <div
                  key={index}
                  onClick={() => {
                    if (activeTab === "read") {
                      navigate(`/detalji-knjige/${k.knjiga.id}`); 
                    } else {
                      setCurrentBook(k); 
                      setShowTrenutna(true); 
                    }
                  }}
                  className="cursor-pointer text-center transition-all transform hover:scale-105"
                >
                  <img
                    src={k.knjiga.slika || 'default_image.jpg'} 
                    alt={k.knjiga.naslov}
                    className="w-32 h-48 object-cover rounded-lg shadow-lg border-8 border-[rgb(224,209,190)]"
                  />
                  <p className="mt-2 font-medium text-black">{k.knjiga.naslov}</p>
                  <p className="mt-2 font-medium text-yellow-900">{k.autor}</p>
                </div>
              ))}
            </div>
          )}

       
          {currentPage < Math.ceil((activeTab === "current" ? knjige : procitaneKnjige).length / 5) - 1 && (
            <button
              onClick={() => setCurrentPage((prev) => prev + 1)}
              className="absolute right-0 p-2 bg-[rgb(224,209,190)] hover:bg-[rgb(218,200,180)] rounded-full transition-all"
            >
              ▶
            </button>
          )}
        </div>
      </div>

      <AzuriranjeProfila
        isOpen={showProfileUpdate}
        onClose={() => setShowProfileUpdate(false)}
        userData={korisnik}
      />


      {showTrenutna && currentBook && (
        <TrenutnaKnjiga
          knjiga={currentBook}
          onClose={() => setShowTrenutna(false)}
        />
      )}
    </div>
  );
};

export default ProfilnaStrana;