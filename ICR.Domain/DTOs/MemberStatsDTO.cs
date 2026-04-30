using ICR.Domain.Model.MemberAggregate;

namespace ICR.Domain.DTOs
{
    public class MemberClassCountDTO
    {
        public ClassType Class { get; set; }
        public string ClassName => Class switch
        {
            ClassType.BEBE => "Bebê",
            ClassType.CRIANCA => "Criança",
            ClassType.JUNIORES => "Juniores",
            ClassType.JUVENIS => "Juvenis",
            ClassType.JOVENS => "Jovens",
            ClassType.HOMENS => "Homens",
            ClassType.MULHERES => "Mulheres",
            _ => "Desconhecido"
        };
        public int Total { get; set; }
    }

    public class MemberRoleCountDTO
    {
        public MemberRole Role { get; set; }
        public string RoleName => Role switch
        {
            MemberRole.Pastor => "Pastor",
            MemberRole.Presbitero => "Presbítero",
            MemberRole.Diacono => "Diácono",
            MemberRole.Obreiro => "Obreiro",
            MemberRole.Midias => "Mídias",
            MemberRole.Louvor => "Louvor",
            MemberRole.Som_Projecao => "Som e Projeção",
            MemberRole.Secretaria_Integracao => "Secretaria e Integração",
            MemberRole.Ensino => "Ensino",
            MemberRole.Evangelizacao_Social => "Evangelização e Social",
            MemberRole.Familias => "Famílias",
            MemberRole.Outros => "Outros",
            _ => "Desconhecido"
        };
        public int Total { get; set; }
    }
}
