//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClinicDent.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Inventario
    {
        public int id_inventario { get; set; }
        public int id_material { get; set; }
        public System.DateTime fecha_actualizacion { get; set; }
    
        public virtual Materiales Materiales { get; set; }
    }
}
