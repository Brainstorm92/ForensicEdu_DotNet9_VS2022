import {useEffect,useRef,useState} from 'react';
import {apiRequest} from '../../services/apiTransport';
import {loadGoogleIdentity} from '../../services/googleIdentity';
import {useTheme} from '../../context/ThemeContext';
interface GoogleConfig{enabled:boolean;clientId:string;nonce:string;}
export function GoogleSignInButton({onCredential,disabled}:{onCredential:(credential:string)=>void;disabled:boolean}){
 const host=useRef<HTMLDivElement>(null);const callback=useRef(onCredential);callback.current=onCredential;const request=useRef<Promise<GoogleConfig>|null>(null);const {theme}=useTheme();const [status,setStatus]=useState('Cargando Google…');
 useEffect(()=>{let active=true;request.current??=apiRequest<GoogleConfig>('/auth/google/config',{credentials:'include'});
  void request.current.then(async config=>{if(!active)return;if(!config.enabled){setStatus('Google no está disponible por ahora.');return;}const google=await loadGoogleIdentity();if(!active||!host.current)return;
   google.initialize({client_id:config.clientId,nonce:config.nonce,callback:response=>{if(active)callback.current(response.credential);},auto_select:false,ux_mode:'popup',context:'signin',use_fedcm_for_button:false});
   host.current.replaceChildren();google.renderButton(host.current,{theme:theme==='dark'?'filled_black':'outline',size:'large',text:'continue_with',shape:'pill',width:Math.min(400,host.current.clientWidth||300),locale:'es'});setStatus('');
  }).catch(e=>{if(active)setStatus(e instanceof Error?e.message:'Google no está disponible por ahora.');});
  return()=>{active=false;};
 },[theme]);
 return <div className="space-y-2"><div ref={host} data-testid="google-signin" aria-label="Continuar con Google" className={`min-h-11 flex justify-center ${disabled?'pointer-events-none opacity-50':''}`}/>{status&&<p className="text-center text-xs text-muted" role="status">{status}</p>}</div>;
}
