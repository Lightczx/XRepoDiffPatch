// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.
using System.CommandLine;

namespace XRepoDiffPatch;

internal static class InvocationOptions
{
    public static readonly Argument<string> GitRoot = new("git-root", "The Git root path to use for the diff generation.");

    public static readonly Argument<string> FromPath = new("from", "The Source Repo path to generate the diff from.");

    public static readonly Argument<string> ToPath = new("to", "The Destination Repo path to patch the diff to.");
}