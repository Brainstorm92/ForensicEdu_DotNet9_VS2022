import {useEffect,useRef,useState} from 'react';
import {CourseVideoControls} from './CourseVideoControls';
import {Notice} from './PlatformUi';
export type VideoPlayback={url:string;expiresAt?:string;privateVideo?:boolean};
export function PrivateVideoPlayer({initial,title,refresh,poster,watermark}:{initial:VideoPlayback;title:string;poster?:string;watermark?:string;refresh:(signal:AbortSignal)=>Promise<VideoPlayback>}){
 const [revision,setRevision]=useState(0);const video=useRef<HTMLVideoElement>(null);const [playback,setPlayback]=useState(initial);const [error,setError]=useState('');const renew=useRef<()=>void>(()=>{});const position=useRef<{time:number;resume:boolean}|null>(null);const failures=useRef(0);const refreshRef=useRef(refresh);refreshRef.current=refresh;
 useEffect(()=>{let live=true;let flight=false;let timer:ReturnType<typeof setTimeout>|undefined;const controller=new AbortController();
  async function update(){if(flight||!live)return;flight=true;try{const next=await refreshRef.current(controller.signal);if(!live)return;const el=video.current;position.current={time:el?.currentTime||0,resume:!!el&&!el.paused&&!el.ended};setPlayback(next);setRevision(v=>v+1);setError('');}catch(e){if(live){video.current?.pause();setError(e instanceof Error?e.message:'No se pudo renovar el acceso al video.');}}finally{flight=false;}}
  renew.current=()=>void update();
  if(playback.expiresAt)timer=setTimeout(()=>void update(),Math.max(5000,new Date(playback.expiresAt).getTime()-Date.now()-60000));
  return()=>{live=false;controller.abort();if(timer)clearTimeout(timer);};
 },[playback.url,playback.expiresAt]);
 return <div className="space-y-2">{error?<div className="p-4 space-y-3"><Notice error>{error}</Notice><button type="button" className="rounded-lg border border-line px-3 py-2 text-sm text-white" onClick={()=>{failures.current=0;renew.current();}}>Reintentar reproducción</button></div>:<CourseVideoControls poster={poster} watermark={watermark} key={revision} ref={video} aria-label={title} className="aspect-video w-full rounded-xl bg-black" preload="metadata" crossOrigin="anonymous" src={playback.url} onLoadedMetadata={()=>{const saved=position.current;if(!saved||!video.current)return;position.current=null;const el=video.current;el.currentTime=Number.isFinite(el.duration)?Math.min(saved.time,Math.max(0,el.duration-.1)):saved.time;if(saved.resume)void el.play().catch(()=>{});}} onCanPlay={()=>{failures.current=0;}} onError={()=>{if(failures.current++===0)renew.current();else setError('No se pudo reproducir este archivo. Comprueba la conexión o utiliza un MP4 H.264 con audio AAC.');}}/>}<p className="text-xs text-muted">Video privado del curso · Acceso temporal autorizado por tu cuenta.</p></div>;
}
