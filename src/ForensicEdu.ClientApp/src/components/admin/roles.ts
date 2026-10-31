import type {UserRole} from '../../types';
export const roles:{id:number;key:UserRole;name:string;level:string;description:string;permissions:string[]}[]=[
 {id:1,key:'superadmin',name:'Superadministrador',level:'L-0',description:'Usuarios, roles y administración general.',permissions:['users','courses','finance','academic','support','audit','social']},
 {id:2,key:'admin_academico',name:'Admin académico',level:'L-1',description:'Cursos, contenidos, evaluaciones y certificados.',permissions:['courses','academic','audit','social']},
 {id:3,key:'admin_financiero',name:'Admin financiero',level:'L-1',description:'Pagos, comprobantes y validación bancaria.',permissions:['finance','audit']},
 {id:4,key:'docente',name:'Docente',level:'L-2',description:'Perfil docente. Sin edición general de cursos.',permissions:[]},
 {id:7,key:'consultor',name:'Consultor pericial',level:'L-2',description:'Perfil de asesoría. Sin permisos de administración general.',permissions:[]},
 {id:5,key:'soporte',name:'Soporte',level:'L-3',description:'Consultas, respuestas y consulta de usuarios.',permissions:['support','users-read']},
 {id:6,key:'estudiante',name:'Estudiante',level:'L-4',description:'Compras, aula, evaluaciones y certificados propios.',permissions:['student']}
];
export const privileges=[['users','Crear usuarios y asignar roles'],['courses','Cursos, módulos, materiales y preguntas'],['finance','Revisar y aprobar pagos'],['academic','Matrículas y revocación de certificados'],['support','Atender consultas'],['users-read','Consultar usuarios'],['audit','Consultar auditoría'],['social','Configurar redes sociales'],['student','Aula y registros propios']];
