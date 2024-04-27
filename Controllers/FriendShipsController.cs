using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MoviesDBManager.Controllers
{
    [OnlineUsers.UserAccess]
    public class FriendShipsController : Controller
    {
        public ActionResult Index()
        {
            Session["LastAction"] = "FriendShips/index";
            return View();
        }
        public ActionResult Relations(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Relations.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged)
            {
                var listesParUser = DB.Relations.ToList().GroupBy(c => c.UsersId.usr1).ToList();
                foreach (var liste in listesParUser) 
                { 
                    if(liste.ElementAt(0).UsersId.usr1 == OnlineUsers.GetSessionUser().Id)
                    {
                        return PartialView(liste.OrderBy(c => DB.Users.Get(c.UsersId.usr2).FirstName));
                    }
                }
            }
            return null;
        }
        public void DemandeAmitié(int Idusr1, int Idusr2)
        {
            var relations = DB.Relations.FindRelation(Idusr1, Idusr2);
            relations.rel1.Rapport = TypeRapport.Demande_Envoyée;
            DB.Relations.Update(relations.rel1);
            relations.rel2.Rapport = TypeRapport.Requête_Entrante;
            DB.Relations.Update(relations.rel2);
        }
        public void AccepterDemande(int Idusr1, int Idusr2)
        {
            var relations = DB.Relations.FindRelation(Idusr1, Idusr2);
            relations.rel1.Rapport = TypeRapport.Amis;
            DB.Relations.Update(relations.rel1);
            relations.rel2.Rapport = TypeRapport.Amis;
            DB.Relations.Update(relations.rel2);
        }
        public void RefuserDemande(int Idusr1, int Idusr2)
        {
            var relations = DB.Relations.FindRelation(Idusr1, Idusr2);
            relations.rel1.Rapport = TypeRapport.A_Refusé_Demande;
            DB.Relations.Update(relations.rel1);
            relations.rel2.Rapport = TypeRapport.Demande_Refusé;
            DB.Relations.Update(relations.rel2);
        }
        public void RetirerDemande(int Idusr1, int Idusr2)
        {
            var relations = DB.Relations.FindRelation(Idusr1, Idusr2);
            relations.rel1.Rapport = TypeRapport.Pas_Encore_Ami;
            DB.Relations.Update(relations.rel1);
            relations.rel2.Rapport = TypeRapport.Pas_Encore_Ami;
            DB.Relations.Update(relations.rel2);
        }
    }
}