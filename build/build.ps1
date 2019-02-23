properties {
   $VersionSuffix = $null
   $BasePath = Resolve-Path ..
   $SrcPath = "$BasePath\src"
   $ArtifactsPath = "$BasePath\artifacts"
   $InMemoryProjectPath = "$SrcPath\Grouchy.ServiceBus.InMemory\Grouchy.ServiceBus.InMemory.csproj"
   $InMemoryTestProjectPath = "$SrcPath\Grouchy.ServiceBus.InMemory.Tests\Grouchy.ServiceBus.InMemory.Tests.csproj"
   $RabbitMQProjectPath = "$SrcPath\Grouchy.ServiceBus.RabbitMQ\Grouchy.ServiceBus.RabbitMQ.csproj"
   $RabbitMQTestProjectPath = "$SrcPath\Grouchy.ServiceBus.RabbitMQ.Tests\Grouchy.ServiceBus.RabbitMQ.Tests.csproj"
   $ServiceBusProjectPath = "$SrcPath\Grouchy.ServiceBus\Grouchy.ServiceBus.csproj"
   $Configuration = if ($Configuration) {$Configuration} else { "Debug" }
}

task default -depends Clean, Build, Test, Package

task Clean {
   if (Test-Path -path $ArtifactsPath)
   {
      Remove-Item -path $ArtifactsPath -Recurse -Force | Out-Null
   }

   New-Item -Path $ArtifactsPath -ItemType Directory
}

task Build {
   exec { dotnet --version }
   exec { dotnet restore $InMemoryProjectPath }
   exec { dotnet restore $RabbitMQProjectPath }
   exec { dotnet restore $ServiceBusProjectPath }

   if ($VersionSuffix -eq $null -or $VersionSuffix -eq "") {
      exec { dotnet build $InMemoryProjectPath -c $Configuration -f netstandard2.0 --no-incremental }
      exec { dotnet build $InMemoryProjectPath -c $Configuration -f netstandard2.0 --no-incremental }
      exec { dotnet build $RabbitMQProjectPath -c $Configuration -f net461 --no-incremental }
      exec { dotnet build $RabbitMQProjectPath -c $Configuration -f net461 --no-incremental }
      exec { dotnet build $ServiceBusProjectPath -c $Configuration -f netstandard2.0 --no-incremental }
   }
   else {
      exec { dotnet build $InMemoryProjectPath -c $Configuration -f netstandard2.0 --no-incremental --version-suffix $VersionSuffix }
      exec { dotnet build $InMemoryProjectPath -c $Configuration -f netstandard2.0 --no-incremental --version-suffix $VersionSuffix }
      exec { dotnet build $RabbitMQProjectPath -c $Configuration -f net461 --no-incremental --version-suffix $VersionSuffix }
      exec { dotnet build $RabbitMQProjectPath -c $Configuration -f net461 --no-incremental --version-suffix $VersionSuffix }
      exec { dotnet build $ServiceBusProjectPath -c $Configuration -f netstandard2.0 --no-incremental --version-suffix $VersionSuffix }
   }
}

task Test -depends Build {
   exec { dotnet restore $InMemoryTestProjectPath }
   exec { dotnet restore $RabbitMQTestProjectPath }
   exec { dotnet test $InMemoryTestProjectPath -c $Configuration -f netcoreapp2.0 --filter Category!=local-only }
   exec { dotnet test $InMemoryTestProjectPath -c $Configuration -f netcoreapp2.0 --filter Category!=local-only }
   exec { dotnet test $RabbitMQTestProjectPath -c $Configuration -f net461 --filter Category!=local-only }
   exec { dotnet test $RabbitMQTestProjectPath -c $Configuration -f net461 --filter Category!=local-only }
}

task Package -depends Build {
   if ($VersionSuffix -eq $null -or $VersionSuffix -eq "") {
      exec { dotnet pack $InMemoryProjectPath -c $Configuration -o $ArtifactsPath }
      exec { dotnet pack $RabbitMQProjectPath -c $Configuration -o $ArtifactsPath }
      exec { dotnet pack $ServiceBusProjectPath -c $Configuration -o $ArtifactsPath }
   }
   else {
      exec { dotnet pack $InMemoryProjectPath -c $Configuration -o $ArtifactsPath --version-suffix $VersionSuffix }
      exec { dotnet pack $RabbitMQProjectPath -c $Configuration -o $ArtifactsPath --version-suffix $VersionSuffix }
      exec { dotnet pack $ServiceBusProjectPath -c $Configuration -o $ArtifactsPath --version-suffix $VersionSuffix }
   }
}
