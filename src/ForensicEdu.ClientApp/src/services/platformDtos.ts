export type ApiEnum = number | string;
export interface CourseDto {id:string;code?:string;shortCode?:string;title:string;category?:string;shortDescription?:string;fullDescription?:string;price:number;discountPrice?:number;academicHours:number;instructor?:string;instructorTitle?:string;instructorImageUrl?:string;modalidad?:string;requisitos?:string;imageUrl?:string;isPublished?:boolean;isActive?:boolean;modules?:ModuleDto[];}
export interface ModuleDto {hasVideo?:boolean;hasPrivateVideo?:boolean;id:string;title:string;duration:string;orderIndex:number;videoStreamingUrl?:string;documents:DocumentDto[];}
export interface DocumentDto {id:string;title:string;fileType:string;fileSize:string;}
export interface EventDto {id:string;code:string;title:string;type:string;description:string;price:number;eventDate:string;timeSlot:string;speaker:string;speakerBio:string;accessStatus:string;bannerUrl:string;}
export interface AdvisoryDto {id:string;title:string;specialist:string;specialty:string;description:string;price:number;whatsappContact?:string;}
export interface OrderDto {id:string;orderCode:string;productId:string;productType:ApiEnum;amount:number;price:number;status:ApiEnum;createdAt:string;payment?:{id:string;status:ApiEnum}|null;}
export interface PaymentDto {id:string;orderId:string;orderCode:string;userId:string;userName?:string;userEmail?:string;productId:string;productType:ApiEnum;productTitle?:string;bankName:string;operationNumber:string;operationDate:string;amount:number;accountHolderName:string;status:ApiEnum;registeredAt:string;reviewedAt?:string;hasVoucher:boolean;}
export interface EnrollmentDto {canReenroll?:boolean;id:string;userId:string;courseId:string;orderId:string;enrolledAt:string;completedModulesJson?:string;status:string;progressPercent:number;examStatus:ApiEnum;firstAttemptScore?:number;secondAttemptScore?:number;finalGrade?:number;attemptsUsed:number;secondAttemptAvailableAt?:string;}
export interface CertificateDto {id:string;enrollmentId:string;studentFullName:string;certificateCode:string;userId?:string;courseId:string;courseTitle:string;academicHours:number;finalGrade:number;status:ApiEnum;issueDate:string;qrValidationUrl:string;}
export interface TicketDto {id:string;ticketCode:string;userId:string;userName:string;userEmail:string;subject:string;category:string;message:string;status:ApiEnum;createdAt:string;responsesJson:string;}
export interface AccountDto {id:string;nombres:string;apellidos:string;fullName:string;email:string;dni:string;phone:string;role:number;status:number;validUntil?:string;hasApprovedPurchase:boolean;registeredAt:string;}
export interface AuditDto {id:string;userId:string;userName:string;userRole:string;action:string;details:string;entityId?:string;createdAt:string;}
export interface PublicQuestion {id:string;question:string;options:string[];}
export interface ExamSessionDto {id:string;attemptNumber:number;startedAt:string;expiresAt:string;serverTime:string;questions:PublicQuestion[];savedAnswers:Record<string,number>;}
export interface ExamResultDto {attemptId:string;attemptNumber:number;score:number;approved:boolean;status:string;certificateCode?:string;message:string;}
export interface ClassroomDto {id:string;courseId:string;course:CourseDto;modules:ModuleDto[];}
