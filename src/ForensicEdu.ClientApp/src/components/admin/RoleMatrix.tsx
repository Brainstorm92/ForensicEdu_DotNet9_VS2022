import {useState} from 'react';
import {useApp} from '../../context/AppContext';
import {roles,privileges} from './roles';
import {Panel} from '../PlatformUi';
export function RoleMatrix(){const {allUsers}=useApp();const [selected,setSelected]=useState(1);const role=roles.find(x=>x.id===selected)!;
 return <div className="space-y-6"><div><p className="admin-eyebrow">GOBERNANZA DE ACCESO</p><h2 className="text-2xl font-bold mt-2">Roles y permisos</h2><p className="text-muted mt-2">Siete perfiles institucionales con funciones y accesos definidos.</p></div>
 <div className="grid sm:grid-cols-2 xl:grid-cols-4 gap-3">{roles.map(r=><button key={r.id} onClick={()=>setSelected(r.id)} className={`admin-role text-left p-5 rounded-xl border ${selected===r.id?'border-brand bg-brand/10':'border-line bg-panel'}`}><span className="flex justify-between gap-2 text-xs text-brand font-bold"><span>{r.level}</span><span>{allUsers.filter(u=>u.role===r.key).length} usuarios</span></span><strong className="block mt-4">{r.name}</strong><span className="text-sm text-muted block mt-2">{r.description}</span></button>)}</div>
 <Panel><h3 className="font-bold text-lg">Privilegios · {role.name}</h3><div className="grid md:grid-cols-2 gap-3 mt-4">{privileges.map(([key,label])=>{const allowed=role.permissions.includes(key)||(key==='users-read'&&role.permissions.includes('users'));return <div key={key} className={`p-4 rounded-xl border ${allowed?'border-brand/30 bg-brand/5':'border-line text-muted'}`}><span className={allowed?'text-brand':'text-muted'}>{allowed?'✓ Habilitado':'— Restringido'}</span><p className="mt-1 font-semibold text-sm">{label}</p></div>;})}</div></Panel>
 <p className="text-xs text-muted">Consulta los permisos de cada rol. Para asignar un rol a una persona, abre Gestión de usuarios.</p></div>;
}
