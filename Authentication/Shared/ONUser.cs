﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authentication
{
    public class ONUser : ClaimsPrincipal
    {
        public const string ROLE_OWNER = "owner";
        public const string ROLE_ADMIN = "admin";
        public const string ROLE_BACKUP = "backup";
        public const string ROLE_OPS = "ops";
        public const string ROLE_SERVICE = "service";
        public const string ROLE_CONTENT_PUBLISHER = "con_publisher";
        public const string ROLE_CONTENT_WRITER = "con_writer";
        public const string ROLE_COMMENT_MODERATOR = "com_mod";
        public const string ROLE_COMMENT_APPELLATE_JUDGE = "com_appellate";
        public const string ROLE_BOT_VERIFICATION = "bot_verification";
        public const string ROLE_EVENT_CREATOR = "evt_creator";
        public const string ROLE_EVENT_MODERATOR = "evt_moderator";

        public const string ROLE_CAN_BACKUP = ROLE_OWNER + "," + ROLE_BACKUP;
        public const string ROLE_CAN_CREATE_CONTENT = ROLE_CAN_PUBLISH + "," + ROLE_CONTENT_WRITER;
        public const string ROLE_CAN_MODERATE_COMMENT = ROLE_IS_COMMENT_MODERATOR_OR_HIGHER;
        public const string ROLE_CAN_PUBLISH =
            ROLE_IS_ADMIN_OR_OWNER + "," + ROLE_CONTENT_PUBLISHER;
        public const string ROLE_IS_ADMIN_OR_OWNER = ROLE_OWNER + "," + ROLE_ADMIN;
        public const string ROLE_IS_ADMIN_OR_OWNER_OR_SERVICE =
            ROLE_IS_ADMIN_OR_OWNER + "," + ROLE_SERVICE;
        public const string ROLE_IS_OWNER_OR_SERVICE = ROLE_SERVICE + "," + ROLE_OWNER;
        public const string ROLE_IS_ADMIN_OR_OWNER_OR_SERVICE_OR_BOT =
            ROLE_IS_ADMIN_OR_OWNER_OR_SERVICE + "," + ROLE_BOT_VERIFICATION;
        public const string ROLE_IS_COMMENT_MODERATOR_OR_HIGHER =
            ROLE_IS_COMMENT_APPELLATE_JUDGE_OR_HIGHER + "," + ROLE_COMMENT_MODERATOR;
        public const string ROLE_IS_COMMENT_APPELLATE_JUDGE_OR_HIGHER =
            ROLE_IS_ADMIN_OR_OWNER + "," + ROLE_COMMENT_APPELLATE_JUDGE;
        public const string ROLE_IS_EVENT_CREATOR_OR_HIGHER =
            ROLE_IS_ADMIN_OR_OWNER + "," + ROLE_EVENT_CREATOR;
        public const string ROLE_IS_EVENT_MODERATOR_OR_HIGHER =
            ROLE_IS_EVENT_CREATOR_OR_HIGHER + "," + ROLE_IS_ADMIN_OR_OWNER;
        public Guid Id { get; set; } = Guid.Empty;
        public const string IdType = "Id";

        public string UserName { get; set; } = "";
        public const string UserNameType = "sub";

        public string DisplayName { get; set; } = "";
        public const string DisplayNameType = "Display";

        public uint SubscriptionLevel { get; set; } = 0;
        public const string SubscriptionLevelType = "SubscriptionLevel";

        public string SubscriptionProvider { get; set; }
        public const string SubscriptionProviderType = "SubscriptionProvider";

        public List<string> Idents { get; private set; } = new List<string>();
        public const string IdentsType = "Idents";

        public List<string> Roles { get; private set; } = new List<string>();
        public const string RolesType = ClaimTypes.Role;

        public bool IsLoggedIn => Id != Guid.Empty;

        public bool IsBackup
        {
            get => IsInRole(ROLE_BACKUP);
        }
        public bool IsOwner
        {
            get => IsInRole(ROLE_OWNER);
        }
        public bool IsAdmin
        {
            get => IsInRole(ROLE_ADMIN);
        }
        public bool IsAdminOrHigher
        {
            get => IsAdmin || IsOwner;
        }
        public bool IsPublisher
        {
            get => IsInRole(ROLE_CONTENT_PUBLISHER);
        }
        public bool IsPublisherOrHigher
        {
            get => IsPublisher || IsAdminOrHigher;
        }
        public bool IsWriter
        {
            get => IsInRole(ROLE_CONTENT_WRITER);
        }
        public bool IsWriterOrHigher
        {
            get => IsWriter || IsPublisherOrHigher;
        }
        public bool IsCommentModerator
        {
            get => IsInRole(ROLE_COMMENT_MODERATOR);
        }
        public bool IsCommentModeratorOrHigher
        {
            get => IsCommentModerator || IsPublisherOrHigher;
        }
        public bool IsCommentAppellateJudge
        {
            get => IsInRole(ROLE_COMMENT_APPELLATE_JUDGE);
        }
        public bool IsCommentAppellateJudgeOrHigher
        {
            get => IsCommentAppellateJudge || IsAdminOrHigher;
        }

        public bool CanPublish
        {
            get => IsPublisherOrHigher;
        }
        public bool CanCreateContent
        {
            get => IsWriterOrHigher;
        }

        public bool CanCreateEvent
        {
            get => IsInRole(ROLE_IS_EVENT_CREATOR_OR_HIGHER);
        }

        public bool CanModerateEvent
        {
            get => IsInRole(ROLE_IS_EVENT_MODERATOR_OR_HIGHER);
        }

        public List<Claim> ExtraClaims { get; private set; } = new List<Claim>();

        public string JwtToken { get; set; } = "";

        public IEnumerable<Claim> ToClaims()
        {
            if (Id != Guid.Empty)
                yield return new Claim(IdType, Id.ToString());

            if (!string.IsNullOrWhiteSpace(UserName))
                yield return new Claim(UserNameType, UserName);

            if (!string.IsNullOrWhiteSpace(DisplayName))
                yield return new Claim(DisplayNameType, DisplayName);

            if (Idents.Count != 0)
                yield return new Claim(IdentsType, string.Join(';', Idents));

            foreach (var r in Roles)
                yield return new Claim(RolesType, r);

            foreach (var c in ExtraClaims)
                yield return c;
        }

        public static ONUser Parse(Claim[] claims)
        {
            if (claims == null || claims.Length == 0)
                return null;

            var user = new ONUser();

            foreach (var claim in claims)
                user.LoadClaim(claim);

            if (!user.IsValid())
                return null;

            return user;
        }

        public override bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }

        private bool IsValid()
        {
            return true; // Id != Guid.Empty;
        }

        private void LoadClaim(Claim claim)
        {
            switch (claim.Type)
            {
                case IdType:
                    Id = Guid.Parse(claim.Value);
                    return;
                case UserNameType:
                    UserName = claim.Value;
                    return;
                case DisplayNameType:
                    DisplayName = claim.Value;
                    return;
                case IdentsType:
                    Idents.AddRange(claim.Value.Split(';'));
                    return;
                case RolesType:
                    Roles.Add(claim.Value);
                    return;
                case SubscriptionLevelType:
                    if (uint.TryParse(claim.Value, out uint i))
                        SubscriptionLevel = i;
                    return;
                case SubscriptionProviderType:
                    SubscriptionProvider = claim.Value;
                    return;
                default:
                    ExtraClaims.Add(claim);
                    return;
            }
        }
    }
}
