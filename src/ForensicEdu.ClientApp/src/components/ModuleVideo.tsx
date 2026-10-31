export function ModuleVideo({url,title}:{url:string;title:string}){
 let embed='';try{const u=new URL(url);const host=u.hostname.toLowerCase();let id='';if(u.protocol==='https:'){
 if(host==='youtu.be')id=u.pathname.split('/')[1]||'';
 else if(['youtube.com','www.youtube.com','m.youtube.com','www.youtube-nocookie.com'].includes(host))id=u.searchParams.get('v')||u.pathname.match(/^\/(?:embed|shorts)\/([^/]+)/)?.[1]||'';
 if(/^[A-Za-z0-9_-]{11}$/.test(id))embed='https://www.youtube-nocookie.com/embed/'+id;
 }}catch{/* Las URL se validan al guardar el módulo. */}
 return embed?<iframe className="aspect-video w-full rounded-xl" src={embed} title={title} allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowFullScreen referrerPolicy="strict-origin-when-cross-origin"/>:<video className="w-full rounded-xl bg-black" controls controlsList="nodownload" src={url}/>;
}
