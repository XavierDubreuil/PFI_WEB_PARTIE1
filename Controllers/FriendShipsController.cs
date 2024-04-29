using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            bool filtersChanged = Session["FiltersChanged"] != null ? (bool)Session["FiltersChanged"] : false;
            List <TypeRapport> ListDeFiltre = new List<TypeRapport>();

            if (forceRefresh || DB.Relations.HasChanged || OnlineUsers.HasChanged() || DB.Users.HasChanged || filtersChanged)
            {
                if (filtersChanged)
                {
                    bool block = false;
                    ListDeFiltre = DoFilter();
                    foreach(var type in ListDeFiltre)
                    {
                        if(type == TypeRapport.block)
                            block = true;
                    }
                    var listesParUser = DB.Relations.ToList().GroupBy(c => c.UsersId.usr1).ToList();
                    List<Relation> ListreFiltrer = new List<Relation>();
                    foreach (var liste in listesParUser)
                    {
                        if (liste.ElementAt(0).UsersId.usr1 == OnlineUsers.GetSessionUser().Id)
                        {
                            foreach (var elem in liste)
                            {
                                var user = DB.Users.Get(elem.UsersId.usr2);
                                if (block && listesParUser.Count() == 0)
                                {
                                    if (user.Blocked)
                                    {
                                        ListreFiltrer.Add(elem);
                                    }
                                }
                                else
                                {
                                    foreach (var item in ListDeFiltre)
                                    {
                                        if (block)
                                        {
                                            if (user.Blocked || elem.Rapport == item)
                                            {
                                                ListreFiltrer.Add(elem);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!user.Blocked && elem.Rapport == item)
                                            {
                                                ListreFiltrer.Add(elem);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return PartialView(ListreFiltrer.OrderBy(c => DB.Users.Get(c.UsersId.usr2).FirstName));
                }
                else
                {
                    var listesParUser = DB.Relations.ToList().GroupBy(c => c.UsersId.usr1).ToList();
                    foreach (var liste in listesParUser)
                    {
                        if (liste.ElementAt(0).UsersId.usr1 == OnlineUsers.GetSessionUser().Id)
                        {
                            return PartialView(liste.OrderBy(c => DB.Users.Get(c.UsersId.usr2).FirstName));
                        }
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
        public ActionResult Filtre(string LesFiltres)
        {
            Session["FiltersChanged"] = true;
            Session["CurrentFilter"] = LesFiltres;
            return null;
        }
        public List<TypeRapport> DoFilter()
        {
            var filtres = ((string)Session["CurrentFilter"]).Split(',');
            List<TypeRapport> listRap = new List<TypeRapport>();
            foreach (var filtre in filtres)
            {
                switch (filtre)
                {
                    case "Pas_Encore_Ami":
                        listRap.Add(TypeRapport.Pas_Encore_Ami);
                        break;
                    case "Requête_Entrante":
                        listRap.Add(TypeRapport.Requête_Entrante);
                        break;
                    case "Demande_Envoyée":
                        listRap.Add(TypeRapport.Demande_Envoyée);
                        break;
                    case "Amis":
                        listRap.Add(TypeRapport.Amis);
                        break;
                    case "DemandeRefuser":
                        listRap.Add(TypeRapport.Demande_Refusé);
                        listRap.Add(TypeRapport.A_Refusé_Demande);
                        break;
                    case "Bloquer":
                        listRap.Add(TypeRapport.block);
                        break;
                }
            }
            return listRap;
        }
    }
}