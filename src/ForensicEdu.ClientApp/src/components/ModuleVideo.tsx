import {CourseVideoControls} from './CourseVideoControls';
export function ModuleVideo({url,title,poster,watermark}:{url:string;title:string;poster?:string;watermark?:string}){
 let embed='';try{const u=new URL(url);const host=u.hostname.toLowerCase();let id='';if(u.protocol==='https:'){
 if(host==='youtu.be')id=u.pathname.split('/')[1]||'';
 else if(['youtube.com','www.youtube.com','m.youtube.com','www.youtube-nocookie.com'].includes(host))id=u.searchParams.get('v')||u.pathname.match(/^\/(?:embed|shorts)\/([^/]+)/)?.[1]||'';
 if(/^[A-Za-z0-9_-]{11}$/.test(id))embed='https://www.youtube-nocookie.com/embed/'+id;
 }}catch{/* External URLs are validated when saving the module. */}
 return embed?<div className="course-external-video"><iframe className="aspect-video w-full" src={embed} title={title} allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowFullScreen referrerPolicy="strict-origin-when-cross-origin"/>{watermark&&<span className="course-video-watermark mark-0" aria-hidden="true">{watermark}</span>}</div>:<CourseVideoControls src={url} aria-label={title} poster={poster} watermark={watermark} preload="metadata"/>;
}
