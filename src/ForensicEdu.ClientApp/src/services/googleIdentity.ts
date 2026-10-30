interface CredentialResponse{credential:string;}
interface GoogleIdentityApi {
 initialize(options:{client_id:string;nonce:string;callback:(response:CredentialResponse)=>void;auto_select:boolean;ux_mode:'popup';context:'signin'|'signup';use_fedcm_for_button:boolean}):void;
 renderButton(element:HTMLElement,options:{theme:'outline'|'filled_black';size:'large';text:'continue_with';shape:'pill';width:number;locale:string}):void;
}
declare global{interface Window{google?:{accounts:{id:GoogleIdentityApi}};}}
let loading:Promise<GoogleIdentityApi>|null=null;
export function loadGoogleIdentity(){
 if(window.google?.accounts.id)return Promise.resolve(window.google.accounts.id);
 if(!loading)loading=new Promise<GoogleIdentityApi>((resolve,reject)=>{
  const script=document.createElement('script');script.src='https://accounts.google.com/gsi/client';script.async=true;script.defer=true;
  script.onload=()=>window.google?.accounts.id?resolve(window.google.accounts.id):reject(new Error('No pudimos cargar el acceso con Google.'));
  script.onerror=()=>{loading=null;script.remove();reject(new Error('No pudimos cargar Google. Revisa tu conexión o usa tu correo.'));};document.head.appendChild(script);
 });return loading;
}
