using System;
using System.Collections.Generic;

namespace LoginService.Models;

public partial class Session
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public bool IsValid { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual User User { get; set; } = null!;
}
