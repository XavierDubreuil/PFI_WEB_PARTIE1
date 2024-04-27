using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public enum TypeRapport { Pas_Encore_Ami, Requête_Entrante, Demande_Envoyée, Amis, Demande_Refusé, A_Refusé_Demande};
    public class Relation
    {
        public int Id { get; set; }
        public (int usr1,int usr2) UsersId { get; set; }
        public TypeRapport Rapport { get; set; }
        public Relation(int userId, int otherUserId)
        {
            UsersId = (userId, otherUserId);
            Rapport = TypeRapport.Pas_Encore_Ami;
        }
        public Relation() { }
    }
}